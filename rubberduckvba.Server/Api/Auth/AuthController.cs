using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Octokit;
using Octokit.Internal;
using System.Security.Claims;
using System.Text;

namespace rubberduckvba.com.Server.Api.Auth;

public record class UserViewModel
{
    public static UserViewModel Anonymous { get; } = new UserViewModel { Name = "(anonymous)", HasOrgRole = false };

    public string Name { get; init; } = default!;
    public bool HasOrgRole { get; init; }
}



[ApiController]
[AllowAnonymous]
public class AuthController(IOptions<GitHubSettings> configuration, IOptions<ApiSettings> api) : ControllerBase
{
    [HttpGet("auth")]
    [AllowAnonymous]
    public ActionResult<UserViewModel> Index()
    {
        var claims = HttpContext.User.Claims.ToDictionary(claim => claim.Type, claim => claim.Value);
        var hasName = claims.TryGetValue(ClaimTypes.Name, out var name);
        var hasRole = claims.TryGetValue(ClaimTypes.Role, out var role);

        if (hasName && hasRole)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(role))
            {
                return BadRequest();
            }

            var model = new UserViewModel
            {
                Name = name,
                HasOrgRole = (HttpContext.User.Identity?.IsAuthenticated ?? false) && role == configuration.Value.OwnerOrg
            };

            return Ok(model);
        }
        else
        {
            return Ok(UserViewModel.Anonymous);
        }
    }

    [HttpPost("signin")]
    [AllowAnonymous]
    public async Task<ActionResult> SignIn()
    {
        var xsrf = Guid.NewGuid().ToString();
        HttpContext.Session.SetString("xsrf:state", xsrf);
        await HttpContext.Session.CommitAsync();

        var clientId = configuration.Value.ClientId;
        var agent = configuration.Value.UserAgent;

        var github = new GitHubClient(new ProductHeaderValue(agent));
        var request = new OauthLoginRequest(clientId)
        {
            AllowSignup = false,
            Scopes = { "read:user", "read:org" },
            State = xsrf
        };

        var url = github.Oauth.GetGitHubLoginUrl(request);
        if (url is null)
        {
            return Forbid();
        }

        // TODO log url
        //return Redirect(url.ToString());
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("github")]
    [AllowAnonymous]
    public async Task<ActionResult> GitHubCallback(string code, string state)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest();
        }

        var expected = HttpContext.Session.GetString("xsrf:state");
        HttpContext.Session.Clear();
        await HttpContext.Session.CommitAsync();

        if (state != expected)
        {
            return BadRequest();
        }

        var clientId = configuration.Value.ClientId;
        var clientSecret = configuration.Value.ClientSecret;
        var agent = configuration.Value.UserAgent;

        var github = new GitHubClient(new ProductHeaderValue(agent));

        var request = new OauthTokenRequest(clientId, clientSecret, code);
        var token = await github.Oauth.CreateAccessToken(request);

        await AuthorizeAsync(token.AccessToken);

        return Ok();
    }

    private async Task AuthorizeAsync(string token)
    {
        try
        {
            var credentials = new Credentials(token);
            var github = new GitHubClient(new ProductHeaderValue(configuration.Value.UserAgent), new InMemoryCredentialStore(credentials));
            var githubUser = await github.User.Current();
            if (githubUser.Suspended)
            {
                throw new InvalidOperationException("User is suspended");
            }

            var emailClaim = new Claim(ClaimTypes.Email, githubUser.Email);

            var identity = new ClaimsIdentity("github", ClaimTypes.Name, ClaimTypes.Role);
            if (identity != null)
            {
                identity.AddClaim(new Claim(ClaimTypes.Name, githubUser.Login));

                var orgs = await github.Organization.GetAllForUser(githubUser.Login);
                var rdOrg = orgs.SingleOrDefault(org => org.Id == configuration.Value.RubberduckOrgId);

                if (rdOrg != null)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, configuration.Value.OwnerOrg));
                    identity.AddClaim(new Claim(ClaimTypes.Authentication, token));
                    identity.AddClaim(new Claim("access_token", token));

                    var principal = new ClaimsPrincipal(identity);

                    var issued = DateTime.UtcNow;
                    var expires = issued.Add(TimeSpan.FromMinutes(50));
                    var roles = string.Join(",", identity.Claims.Where(claim => claim.Type == ClaimTypes.Role).Select(claim => claim.Value));

                    HttpContext.User = principal;
                    Thread.CurrentPrincipal = HttpContext.User;

                    var jwt = principal.AsJWT(api.Value.SymetricKey, configuration.Value.JwtIssuer, configuration.Value.JwtAudience);
                    HttpContext.Session.SetString("jwt", jwt);
                }
            }
        }
        catch (Exception)
        {
            // just ignore: configuration needs the org (prod) client app id to avoid throwing this exception
        }
    }
}
