using rubberduckvba.Server.Api.Admin;
using System.Security.Cryptography;
using System.Text;

namespace rubberduckvba.Server;

public class WebhookSignatureValidationService(ConfigurationOptions configuration)
{
    public bool Validate(
        string payload,
        string? userAgent,
        string[] xGitHubEvent,
        string[] xGitHubDelivery,
        string[] xHubSignature,
        string[] xHubSignature256
        )
    {
        if (!(userAgent ?? string.Empty).StartsWith("GitHub-Hookshot/"))
        {
            // user agent must be GitHub hookshot
            return false;
        }

        if (!xGitHubEvent.Contains("push"))
        {
            // only authenticate push events
            return false;
        }

        if (!Guid.TryParse(xGitHubDelivery.SingleOrDefault(), out _))
        {
            // delivery should parse as a GUID
            return false;
        }

        if (!xHubSignature.Any())
        {
            // SHA-1 signature header must be present
            return false;
        }

        var signature = xHubSignature256.SingleOrDefault();
        if (signature == default)
        {
            // SHA-256 signature header must be present
            return false;
        }

        if (!IsValidSignature(signature, payload))
        {
            // SHA-256 signature must match
            return false;
        }

        return true;
    }

    private bool IsValidSignature(string? signature, string payload)
    {
        if (string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }
        using var sha256 = SHA256.Create();

        var secret = configuration.GitHubOptions.Value.WebhookToken;
        var bytes = Encoding.UTF8.GetBytes(secret + payload);
        var check = $"sha256={Encoding.UTF8.GetString(sha256.ComputeHash(bytes))}";

        return signature == check;
    }
}
