using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using rubberduckvba.Server.Api.Auth;
using rubberduckvba.Server.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace rubberduckvba.Server;

public class GitHubAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public static readonly string AuthTokenHeader = "x-access-token";
    public static readonly string AuthCookie = "x-auth";

    private readonly IGitHubClientService _github;

    private readonly string _audience;
    private readonly string _issuer;
    private readonly string _secret;

    public GitHubAuthenticationHandler(IGitHubClientService github, IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger,
        UrlEncoder encoder, IOptions<ApiSettings> apiOptions)
        : base(options, logger, encoder)
    {
        _github = github;

        _audience = apiOptions.Value.Audience;
        _issuer = apiOptions.Value.Issuer;
        _secret = apiOptions.Value.SymetricKey;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            if (TryAuthenticateJWT(out var jwtResult))
            {
                return Task.FromResult(jwtResult!);
            }

            var token = Context.Request.Headers[AuthTokenHeader].SingleOrDefault();
            if (!string.IsNullOrEmpty(token))
            {
                if (TryAuthenticateGitHubToken(token, out var result)
                    && result is AuthenticateResult
                    && result.Ticket is AuthenticationTicket)
                {
                    return Task.FromResult(result!);
                }
            }

            return Task.FromResult(AuthenticateResult.Fail("Missing or invalid access token"));
        }
        catch (InvalidOperationException e)
        {
            Logger.LogError(e, "{Message}", e.Message);
            return Task.FromResult(AuthenticateResult.NoResult());
        }
    }

    private bool TryAuthenticateJWT(out AuthenticateResult? result)
    {
        result = null;

        var jsonContent = Context.Request.Cookies[AuthCookie];
        if (!string.IsNullOrEmpty(jsonContent))
        {
            var payload = JwtPayload.Deserialize(jsonContent);
            if (!payload.Iss.Equals(_issuer, StringComparison.OrdinalIgnoreCase))
            {
                Logger.LogWarning("Invalid issuer in JWT payload: {Issuer}", payload.Iss);
                return false;
            }
            if (!payload.Aud.Contains(_audience))
            {
                Logger.LogWarning("Invalid audience in JWT payload: {Audience}", payload.Aud);
                return false;
            }

            var principal = payload.ToClaimsPrincipal();
            Context.User = principal;
            Thread.CurrentPrincipal = principal;

            var ticket = new AuthenticationTicket(principal, "github");
            result = AuthenticateResult.Success(ticket);
            return true;
        }

        return false;
    }

    private bool TryAuthenticateGitHubToken(string token, out AuthenticateResult? result)
    {
        var task = AuthenticateGitHubAsync(token);
        result = task.GetAwaiter().GetResult();

        return result is not null;
    }

    private async Task<AuthenticateResult?> AuthenticateGitHubAsync(string token)
    {
        var principal = await _github.ValidateTokenAsync(token);
        if (principal is ClaimsPrincipal)
        {
            Context.User = principal;
            Thread.CurrentPrincipal = principal;

            var jwt = principal.ToJWT(_secret, _issuer, _audience);
            Context.Response.Cookies.Append(AuthCookie, jwt, new CookieOptions
            {
                IsEssential = true,
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            var ticket = new AuthenticationTicket(principal, "github");
            return AuthenticateResult.Success(ticket);
        }
        return null;
    }
}
