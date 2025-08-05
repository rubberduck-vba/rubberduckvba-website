using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Octokit;
using Octokit.Internal;
using System.Security.Claims;

namespace rubberduckvba.Server.Api.Auth;

public record class UserViewModel
{
    public static UserViewModel Anonymous { get; } = new UserViewModel { Name = "(anonymous)", IsAuthenticated = false, IsAdmin = false, IsReviewer = false, IsWriter = false };

    public string Name { get; init; } = default!;
    public bool IsAuthenticated { get; init; }
    public bool IsAdmin { get; init; }
    public bool IsReviewer { get; init; }
    public bool IsWriter { get; init; }
}

public record class SignInViewModel
{
    public string? State { get; init; }
    public string? Code { get; init; }
    public string? Token { get; init; }
}

[ApiController]
[AllowAnonymous]
public class AuthController : RubberduckApiController
{
    private readonly IOptions<GitHubSettings> configuration;

    public AuthController(IOptions<GitHubSettings> configuration, IOptions<ApiSettings> api, ILogger<AuthController> logger)
        : base(logger)
    {
        this.configuration = configuration;
    }

    [HttpGet("auth")]
    [EnableCors(CorsPolicies.AllowAll)]
    [AllowAnonymous]
    public IActionResult Index()
    {
        return GuardInternalAction(() =>
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
                    IsAuthenticated = isAuthenticated,
                    IsAdmin = role == RDConstants.Roles.AdminRole,
                    IsReviewer = role == RDConstants.Roles.AdminRole || role == RDConstants.Roles.ReviewerRole,
                    IsWriter = role == RDConstants.Roles.WriterRole || role == RDConstants.Roles.AdminRole || role == RDConstants.Roles.ReviewerRole,
                };

                return Ok(model);
            }
            else
            {
                return Ok(UserViewModel.Anonymous);
            }
        });
    }

    [HttpPost("auth/signin")]
    [EnableCors(CorsPolicies.AllowAll)]
    [AllowAnonymous]
    public IActionResult SessionSignIn(SignInViewModel vm)
    {
        return GuardInternalAction(() =>
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                Logger.LogInformation("Signin was requested, but user is already authenticated. Redirecting to home page...");
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

            Logger.LogInformation("Requesting OAuth app GitHub login url...");
            var url = github.Oauth.GetGitHubLoginUrl(request);
            if (url is null)
            {
                Logger.LogInformation("OAuth login was cancelled by the user or did not return a url.");
                return Forbid();
            }

            Logger.LogInformation("Returning the login url for the client to redirect. State: {xsrf}", vm.State);
            return Ok(url.ToString());
        });
    }

    [HttpPost("auth/github")]
    [EnableCors(CorsPolicies.AllowAll)]
    [AllowAnonymous]
    public async Task<IActionResult> OnGitHubCallback(SignInViewModel vm)
    {
        return await GuardInternalAction(async () =>
        {
            Logger.LogInformation("OAuth code was received. State: {state}", vm.State);
            var clientId = configuration.Value.ClientId;
            var clientSecret = configuration.Value.ClientSecret;
            var agent = configuration.Value.UserAgent;

            var github = new GitHubClient(new ProductHeaderValue(agent));
            var request = new OauthTokenRequest(clientId, clientSecret, vm.Code);

            var token = await github.Oauth.CreateAccessToken(request);

            if (token is null)
            {
                Logger.LogWarning("OAuth access token was not created.");
                return Unauthorized();
            }

            Logger.LogInformation("OAuth access token was created. Authorizing...");
            var authorizedToken = await AuthorizeAsync(token.AccessToken);

            return authorizedToken is null ? Unauthorized() : Ok(vm with { Token = authorizedToken });
        });
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
                Logger.LogWarning("User login '{login}' ({name}) is a suspended GitHub account and will not be authorized.", githubUser.Login, githubUser.Name);
                return default;
            }

            var identity = new ClaimsIdentity("github", ClaimTypes.Name, ClaimTypes.Role);
            identity.AddClaim(new Claim(ClaimTypes.Name, githubUser.Login));
            Logger.LogInformation("Creating claims identity for GitHub login '{login}'...", githubUser.Login);

            var orgs = await github.Organization.GetAllForUser(githubUser.Login);
            var rdOrg = orgs.SingleOrDefault(org => org.Id == configuration.Value.RubberduckOrgId);

            if (rdOrg != null)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, configuration.Value.OwnerOrg));
                identity.AddClaim(new Claim(ClaimTypes.Authentication, token));
                identity.AddClaim(new Claim("access_token", token));
                Logger.LogDebug("GitHub Organization claims were granted. Creating claims principal...");

                var principal = new ClaimsPrincipal(identity);
                var roles = string.Join(",", identity.Claims.Where(claim => claim.Type == ClaimTypes.Role).Select(claim => claim.Value));

                HttpContext.User = principal;
                Thread.CurrentPrincipal = HttpContext.User;

                Logger.LogInformation("GitHub user with login {login} has signed in with role authorizations '{role}'.", githubUser.Login, configuration.Value.OwnerOrg);
                Response.Cookies.Append(GitHubAuthenticationHandler.AuthTokenHeader, token, new CookieOptions
                {
                    IsEssential = true,
                    HttpOnly = true,
                    Secure = true,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                });
                return token;
            }
            else
            {
                Logger.LogWarning("User {name} ({email}) with login '{login}' is not a member of organization ID {org} and will not be authorized.", githubUser.Name, githubUser.Email, githubUser.Login, configuration.Value.RubberduckOrgId);
                return default;
            }
        }
        catch (Exception)
        {
            // just ignore: configuration needs the org (prod) client app id to avoid throwing this exception
            Logger.LogWarning("An exception was thrown. Verify GitHub:ClientId and GitHub:ClientSecret configuration; authorization fails.");
            return default;
        }
    }
}
