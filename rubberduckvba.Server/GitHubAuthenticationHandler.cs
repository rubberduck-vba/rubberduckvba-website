using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using rubberduckvba.Server.Services;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace rubberduckvba.Server;

public class GitHubAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public static readonly string AuthCookie = "x-access-token";

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

    private static readonly ConcurrentDictionary<string, Task<AuthenticateResult?>> _authApiTask = new();

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            var token = Context.Request.Cookies[AuthCookie]
                        ?? Context.Request.Headers[AuthCookie];

            if (string.IsNullOrWhiteSpace(token))
            {
                return Task.FromResult(AuthenticateResult.Fail("Access token was not provided"));
            }

            if (TryAuthenticateFromCache(token, out var cachedResult))
            {
                return Task.FromResult(cachedResult)!;
            }

            if (TryAuthenticateGitHubToken(token, out var result)
                && result is AuthenticateResult
                && result.Ticket is AuthenticationTicket ticket)
            {
                CacheAuthenticatedTicket(token, ticket);
                return Task.FromResult(result!);
            }

            if (TryAuthenticateFromCache(token, out cachedResult))
            {
                return Task.FromResult(cachedResult!);
            }

            return Task.FromResult(AuthenticateResult.Fail("Missing or invalid access token"));
        }
        catch (InvalidOperationException e)
        {
            Logger.LogError(e, e.Message);
            return Task.FromResult(AuthenticateResult.NoResult());
        }
    }

    private void CacheAuthenticatedTicket(string token, AuthenticationTicket ticket)
    {
        if (!string.IsNullOrWhiteSpace(token) && ticket.Principal.Identity?.IsAuthenticated == true)
        {
            _cache.Set(token, ticket, _options);
        }
    }

    private bool TryAuthenticateFromCache(string token, out AuthenticateResult? result)
    {
        result = null;
        if (_cache.TryGetValue(token, out var cached) && cached is AuthenticationTicket cachedTicket)
        {
            var cachedPrincipal = cachedTicket.Principal;

            Context.User = cachedPrincipal;
            Thread.CurrentPrincipal = cachedPrincipal;

            Logger.LogInformation($"Successfully retrieved authentication ticket from cached token for {cachedPrincipal.Identity!.Name}; token will not be revalidated.");
            result = AuthenticateResult.Success(cachedTicket);
            return true;
        }
        return false;
    }

    private bool TryAuthenticateGitHubToken(string token, out AuthenticateResult? result)
    {
        result = null;
        if (_authApiTask.TryGetValue(token, out var task) && task is not null)
        {
            result = task.GetAwaiter().GetResult();
            return result is not null;
        }

        _authApiTask[token] = AuthenticateGitHubAsync(token);
        result = _authApiTask[token].GetAwaiter().GetResult();

        _authApiTask[token] = null!;
        return result is not null;
    }

    private async Task<AuthenticateResult?> AuthenticateGitHubAsync(string token)
    {
        var principal = await _github.ValidateTokenAsync(token);
        if (principal is ClaimsPrincipal)
        {
            Context.User = principal;
            Thread.CurrentPrincipal = principal;

            var ticket = new AuthenticationTicket(principal, "github");
            return AuthenticateResult.Success(ticket);
        }
        return null;
    }
}
