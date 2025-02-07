using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Octokit;
using Octokit.Internal;
using System.Security.Claims;

namespace rubberduckvba.Server.Api.Auth;

public record class UserViewModel
{
    public static UserViewModel Anonymous { get; } = new UserViewModel { Name = "(anonymous)", IsAuthenticated = false, IsAdmin = false };

    public string Name { get; init; } = default!;
    public bool IsAuthenticated { get; init; }
    public bool IsAdmin { get; init; }
}

public record class SignInViewModel
{
    public string? State { get; init; }
    public string? Code { get; init; }
    public string? Token { get; init; }
}

[ApiController]
public class AuthController(IOptions<GitHubSettings> configuration, IOptions<ApiSettings> api, ILogger<AuthController> logger) : ControllerBase
{
    [HttpGet("auth")]
    public IActionResult Index()
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

            var isAuthenticated = HttpContext.User.Identity?.IsAuthenticated ?? false;
            var model = new UserViewModel
            {
                Name = name,
                IsAuthenticated = true,
                IsAdmin = role == configuration.Value.OwnerOrg
            };

            return Ok(model);
        }
        else
        {
            return Ok(UserViewModel.Anonymous);
        }
    }

    [HttpPost("auth/signin")]
    public IActionResult SessionSignIn(SignInViewModel vm)
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            logger.LogInformation("Signin was requested, but user is already authenticated. Redirecting to home page...");
            return Redirect("/");
        }

        var clientId = configuration.Value.ClientId;
        var agent = configuration.Value.UserAgent;

        var github = new GitHubClient(new ProductHeaderValue(agent));
        var request = new OauthLoginRequest(clientId)
        {
            AllowSignup = false,
            Scopes = { "read:user", "read:org" },
            State = vm.State
        };

        logger.LogInformation("Requesting OAuth app GitHub login url...");
        var url = github.Oauth.GetGitHubLoginUrl(request);
        if (url is null)
        {
            logger.LogInformation("OAuth login was cancelled by the user or did not return a url.");
            return Forbid();
        }

        logger.LogInformation("Returning the login url for the client to redirect. State: {xsrf}", vm.State);
        return Ok(url.ToString());
    }

    [HttpPost("auth/github")]
    public async Task<IActionResult> OnGitHubCallback(SignInViewModel vm)
    {
        logger.LogInformation("OAuth token was received. State: {state}", vm.State);
        var clientId = configuration.Value.ClientId;
        var clientSecret = configuration.Value.ClientSecret;
        var agent = configuration.Value.UserAgent;

        var github = new GitHubClient(new ProductHeaderValue(agent));

        var request = new OauthTokenRequest(clientId, clientSecret, vm.Code);
        var token = await github.Oauth.CreateAccessToken(request);
        if (token is null)
        {
            logger.LogWarning("OAuth access token was not created.");
            return Unauthorized();
        }

        logger.LogInformation("OAuth access token was created. Authorizing...");
        var authorizedToken = await AuthorizeAsync(token.AccessToken);
        return authorizedToken is null ? Unauthorized() : Ok(vm with { Token = authorizedToken });
    }

    private async Task<string?> AuthorizeAsync(string token)
    {
        try
        {
            var credentials = new Credentials(token);
            var github = new GitHubClient(new ProductHeaderValue(configuration.Value.UserAgent), new InMemoryCredentialStore(credentials));
            var githubUser = await github.User.Current();
            if (githubUser.Suspended)
            {
                logger.LogWarning("User {name} with login '{login}' ({url}) is a suspended GitHub account and will not be authorized.", githubUser.Name, githubUser.Login, githubUser.Url);
                return default;
            }

            var identity = new ClaimsIdentity("github", ClaimTypes.Name, ClaimTypes.Role);
            identity.AddClaim(new Claim(ClaimTypes.Name, githubUser.Login));
            logger.LogInformation("Creating claims identity for GitHub login '{login}'...", githubUser.Login);

            var orgs = await github.Organization.GetAllForUser(githubUser.Login);
            var rdOrg = orgs.SingleOrDefault(org => org.Id == configuration.Value.RubberduckOrgId);

            if (rdOrg != null)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, configuration.Value.OwnerOrg));
                identity.AddClaim(new Claim(ClaimTypes.Authentication, token));
                identity.AddClaim(new Claim("access_token", token));
                logger.LogDebug("GitHub Organization claims were granted. Creating claims principal...");

                var principal = new ClaimsPrincipal(identity);
                var roles = string.Join(",", identity.Claims.Where(claim => claim.Type == ClaimTypes.Role).Select(claim => claim.Value));

                HttpContext.User = principal;
                Thread.CurrentPrincipal = HttpContext.User;

                logger.LogInformation("GitHub user with login {login} has signed in with role authorizations '{role}'.", githubUser.Login, configuration.Value.OwnerOrg);
                return token;
            }
            else
            {
                logger.LogWarning("User {name} ({email}) with login '{login}' is not a member of organization ID {org} and will not be authorized.", githubUser.Name, githubUser.Email, githubUser.Login, configuration.Value.RubberduckOrgId);
                return default;
            }
        }
        catch (Exception)
        {
            // just ignore: configuration needs the org (prod) client app id to avoid throwing this exception
            logger.LogWarning("An exception was thrown. Verify GitHub:ClientId and GitHub:ClientSecret configuration; authorization fails.");
            return default;
        }
    }
}
