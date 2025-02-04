
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using rubberduckvba.Server.Api.Admin;
using rubberduckvba.Server.Services;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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
        var token = Context.Request.Headers["X-ACCESS-TOKEN"].SingleOrDefault();
        if (token is null)
        {
            return AuthenticateResult.NoResult();
        }

        var principal = await _github.ValidateTokenAsync(token);
        return principal is ClaimsPrincipal
            ? AuthenticateResult.Success(new AuthenticationTicket(principal, "github"))
            : AuthenticateResult.NoResult();
    }
}

public class WebhookAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ConfigurationOptions _configuration;

    public WebhookAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder,
        ConfigurationOptions configuration)
        : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        return await Task.Run(() =>
        {
            var xGitHubEvent = Context.Request.Headers["X-GitHub-Event"];
            var xGitHubDelivery = Context.Request.Headers["X-GitHub-Delivery"];
            var xHubSignature = Context.Request.Headers["X-Hub-Signature"];
            var xHubSignature256 = Context.Request.Headers["X-Hub-Signature-256"];

            if (!xGitHubEvent.Contains("push"))
            {
                // only authenticate push events
                return AuthenticateResult.NoResult();
            }

            if (!Guid.TryParse(xGitHubDelivery.SingleOrDefault(), out _))
            {
                // delivery should parse as a GUID
                return AuthenticateResult.NoResult();
            }

            if (!xHubSignature.Any())
            {
                // signature header should be present
                return AuthenticateResult.NoResult();
            }

            var signature = xHubSignature256.SingleOrDefault();

            using var reader = new StreamReader(Context.Request.Body);
            var payload = reader.ReadToEndAsync().GetAwaiter().GetResult();

            if (!IsValidSignature(signature, payload))
            {
                // encrypted signature must be present
                return AuthenticateResult.NoResult();
            }

            var identity = new ClaimsIdentity("webhook", ClaimTypes.Name, ClaimTypes.Role);
            identity.AddClaim(new Claim(ClaimTypes.Name, "rubberduck-vba-releasebot"));
            identity.AddClaim(new Claim(ClaimTypes.Role, "rubberduck-webhook"));
            identity.AddClaim(new Claim(ClaimTypes.Authentication, "webhook-signature"));

            var principal = new ClaimsPrincipal(identity);
            return AuthenticateResult.Success(new AuthenticationTicket(principal, "webhook-signature"));
        });
    }

    private bool IsValidSignature(string? signature, string payload)
    {
        if (string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        using var sha256 = SHA256.Create();

        var secret = _configuration.GitHubOptions.Value.WebhookToken;
        var bytes = Encoding.UTF8.GetBytes(secret + payload);
        var check = $"sha256={Encoding.UTF8.GetString(sha256.ComputeHash(bytes))}";

        return check == payload;
    }
}