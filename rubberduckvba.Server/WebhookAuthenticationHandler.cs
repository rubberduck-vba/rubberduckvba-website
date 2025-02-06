
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace rubberduckvba.Server;

public class WebhookAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly WebhookHeaderValidationService _service;

    public WebhookAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder,
        WebhookHeaderValidationService service)
        : base(options, logger, encoder)
    {
        _service = service;
    }

    protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        return await Task.Run(() =>
        {
            var userAgent = Context.Request.Headers.UserAgent;
            var xGitHubEvent = Context.Request.Headers["X-GitHub-Event"].OfType<string>().ToArray();
            var xGitHubDelivery = Context.Request.Headers["X-GitHub-Delivery"].OfType<string>().ToArray();
            var xHubSignature = Context.Request.Headers["X-Hub-Signature"].OfType<string>().ToArray();
            var xHubSignature256 = Context.Request.Headers["X-Hub-Signature-256"].OfType<string>().ToArray();

            if (_service.Validate(userAgent, xGitHubEvent, xGitHubDelivery, xHubSignature, xHubSignature256))
            {
                var principal = CreatePrincipal();
                var ticket = new AuthenticationTicket(principal, "webhook-signature");

                return AuthenticateResult.Success(ticket);
            }

            return AuthenticateResult.NoResult();
        });
    }

    private static ClaimsPrincipal CreatePrincipal()
    {
        var identity = new ClaimsIdentity("webhook", ClaimTypes.Name, ClaimTypes.Role);

        identity.AddClaim(new Claim(ClaimTypes.Name, "rubberduck-vba-releasebot"));
        identity.AddClaim(new Claim(ClaimTypes.Role, "rubberduck-webhook"));
        identity.AddClaim(new Claim(ClaimTypes.Authentication, "webhook-signature"));

        var principal = new ClaimsPrincipal(identity);
        return principal;
    }
}