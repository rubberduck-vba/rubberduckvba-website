using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace rubberduckvba.Server.Api.Admin;

[ApiController]
public class WebhookController : RubberduckApiController
{
    private readonly WebhookPayloadValidationService _validator;
    private readonly HangfireLauncherService _hangfire;

    public WebhookController(
        ILogger<WebhookController> logger,
        HangfireLauncherService hangfire,
        WebhookPayloadValidationService validator)
        : base(logger)
    {
        _validator = validator;
        _hangfire = hangfire;
    }

    [Authorize("webhook")]
    [EnableCors(CorsPolicies.AllowAll)]
    [HttpPost("webhook/github")]
    public async Task<IActionResult> GitHub([FromBody] dynamic body) =>
        GuardInternalAction(() =>
        {
            string json = body.ToString();

            var payload = JsonSerializer.Deserialize<PushWebhookPayload>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidOperationException("Could not deserialize payload");
            var eventType = _validator.Validate(payload, json, Request.Headers, out var content, out var gitref);

            if (eventType == WebhookPayloadType.Push)
            {
                var jobId = _hangfire.UpdateXmldocContent();
                var message = $"Webhook push event was accepted. Tag '{gitref?.Name}' associated to the payload will be processed by JobId '{jobId}'.";

                Logger.LogInformation(message);
                return Ok(message);
            }
            else if (eventType == WebhookPayloadType.Ping)
            {
                Logger.LogInformation("Webhook ping event was accepted; nothing to process.");
                return Ok();
            }
            else if (eventType == WebhookPayloadType.Greeting)
            {
                Logger.LogInformation("Webhook push event was accepted; nothing to process. {content}", content);
                return string.IsNullOrWhiteSpace(content) ? NoContent() : Ok(content);
            }

            // reject the payload
            return BadRequest();
        });
}
