
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using rubberduckvba.Server.Services;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace rubberduckvba.Server;

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
        try
        {
            var token = Context.Request.Headers["X-ACCESS-TOKEN"].SingleOrDefault();
            if (string.IsNullOrWhiteSpace(token))
            {
                return AuthenticateResult.Fail("Access token was not provided");
            }

            var principal = await _github.ValidateTokenAsync(token);
            if (principal is ClaimsPrincipal)
            {
                Context.User = principal;
                Thread.CurrentPrincipal = principal;
                return AuthenticateResult.Success(new AuthenticationTicket(principal, "github"));
            }

            return AuthenticateResult.Fail("An invalid access token was provided");
        }
        catch (InvalidOperationException e)
        {
            Logger.LogError(e, e.Message);
            return AuthenticateResult.NoResult();
        }
    }
}
