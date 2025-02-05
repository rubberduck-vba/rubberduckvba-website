namespace rubberduckvba.Server.Api.Admin;

public class WebhookPayloadValidationService(ConfigurationOptions options)
{
    public WebhookPayloadType Validate(PushWebhookPayload payload, IHeaderDictionary headers, out string? content, out GitRef? gitref)
    {
        content = default;

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

        return WebhookPayloadType.Unsupported;
    }
}
