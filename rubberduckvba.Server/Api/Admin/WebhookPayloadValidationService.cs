using Newtonsoft.Json.Linq;

namespace rubberduckvba.Server.Api.Admin;

public class WebhookPayloadValidationService(ConfigurationOptions options)
{
    public WebhookPayloadType Validate(JToken payload, IHeaderDictionary headers, out string? content, out GitRef? gitref)
    {
        content = default;
        gitref = default;

        if (!IsValidHeaders(headers) || !IsValidSource(payload) || !IsValidEvent(payload))
        {
            return WebhookPayloadType.Unsupported;
        }

        gitref = new GitRef(payload.Value<string>("ref"));
        if (!(payload.Value<bool>("created") && gitref.HasValue && gitref.Value.IsTag))
        {
            content = payload.Value<string>("zen");
            return WebhookPayloadType.Greeting;
        }

        return WebhookPayloadType.Push;
    }

    private bool IsValidHeaders(IHeaderDictionary headers) =>
        headers.TryGetValue("X-GitHub-Event", out Microsoft.Extensions.Primitives.StringValues values) && values.Contains("push");

    private bool IsValidSource(JToken payload) =>
        payload["repository"].Value<string>("name") == options.GitHubOptions.Value.Rubberduck &&
        payload["owner"].Value<int>("id") == options.GitHubOptions.Value.RubberduckOrgId;

    private bool IsValidEvent(JToken payload)
    {
        var ev = payload["hook"]?["events"]?.Values<string>() ?? [];
        return ev.Contains("push");
    }
}
