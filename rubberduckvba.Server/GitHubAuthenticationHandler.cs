
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using rubberduckvba.Server.Services;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace rubberduckvba.Server;

public class GitHubAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IGitHubClientService _github;
    private readonly IMemoryCache _cache;

    private static readonly MemoryCacheEntryOptions _options = new MemoryCacheEntryOptions
    {
        SlidingExpiration = TimeSpan.FromMinutes(60),
    };

    public GitHubAuthenticationHandler(IGitHubClientService github, IMemoryCache cache,
        IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
        _github = github;
        _cache = cache;
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

            if (_cache.TryGetValue(token, out var cached) && cached is AuthenticationTicket cachedTicket)
            {
                var cachedPrincipal = cachedTicket.Principal;

                Context.User = cachedPrincipal;
                Thread.CurrentPrincipal = cachedPrincipal;

                Logger.LogInformation($"Successfully retrieved authentication ticket from cached token for {cachedPrincipal.Identity!.Name}; token will not be revalidated.");
                return AuthenticateResult.Success(cachedTicket);
            }

            var principal = await _github.ValidateTokenAsync(token);
            if (principal is ClaimsPrincipal)
            {
                Context.User = principal;
                Thread.CurrentPrincipal = principal;

                var ticket = new AuthenticationTicket(principal, "github");
                _cache.Set(token, ticket, _options);

                return AuthenticateResult.Success(ticket);
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
