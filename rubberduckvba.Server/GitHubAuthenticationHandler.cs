
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using rubberduckvba.com.Server.Services;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace rubberduckvba.com.Server;

public class GitHubAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IGitHubClientService _github;

    public GitHubAuthenticationHandler(IGitHubClientService github,
        IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) 
        : base(options, logger, encoder)
    {
        _github = github;
    }

    protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var token = Context.Request.Headers["X-ACCESS-TOKEN"].SingleOrDefault();
        if (token is null)
        {
            return AuthenticateResult.Fail("No token was provided");
        }

        var principal = await _github.ValidateTokenAsync(token);
        return principal is ClaimsPrincipal
            ? AuthenticateResult.Success(new AuthenticationTicket(principal, "github"))
            : AuthenticateResult.Fail("Could not validate token");
    }
}
