namespace rubberduckvba.Server.Api.Admin;

public class WebhookPayloadValidationService(ConfigurationOptions options, WebhookSignatureValidationService signatureValidation)
{
    public WebhookPayloadType Validate(PushWebhookPayload payload, string body, IHeaderDictionary headers, out string? content, out GitRef? gitref)
    {
        content = default;
        if (!signatureValidation.Validate(body, headers["X-Hub-Signature-256"].OfType<string>().ToArray()))
        {
            gitref = default;
            return WebhookPayloadType.BadRequest;
        }

        gitref = new GitRef(payload.Ref);
        if (headers["X-GitHub-Event"].FirstOrDefault() == "ping")
        {
            if (!(payload.Created && gitref.Value.IsTag))
            {
                return WebhookPayloadType.Greeting;
            }
            return WebhookPayloadType.Ping;
        }

        if (headers["X-GitHub-Event"].FirstOrDefault() == "push")
        {
            return WebhookPayloadType.Push;
        }

        return WebhookPayloadType.BadRequest;
    }
}
