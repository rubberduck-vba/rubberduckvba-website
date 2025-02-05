using Newtonsoft.Json;
using rubberduckvba.Server.Api.Admin;
using System.Security.Cryptography;
using System.Text;

namespace rubberduckvba.Server;

public class WebhookSignatureValidationService(ConfigurationOptions configuration, ILogger<WebhookSignatureValidationService> logger)
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
            if (xGitHubEvent.Contains("ping"))
            {
                // no harm just returning 200-OK on ping
                return true;
            }

            // only authenticate ping and push events
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

        var secret = configuration.GitHubOptions.Value.WebhookToken;
        if (string.IsNullOrWhiteSpace(secret))
        {
            logger.LogWarning("Webhook secret was not found; signature will not be validated.");
            return false;
        }

        var secretBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(payload)));

        using var digest = new HMACSHA256(secretBytes);
        var hash = digest.ComputeHash(payloadBytes);

        var check = $"sha256={Convert.ToHexString(hash).ToLowerInvariant()}";

        return (signature == check);
    }
}
