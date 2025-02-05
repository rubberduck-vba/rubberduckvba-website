namespace rubberduckvba.Server.Api.Admin;

public record class PushWebhookPayload
{
    public string Ref { get; set; }

    public bool Created { get; set; }
    public bool Deleted { get; set; }
    public bool Forced { get; set; }

    public WebhookPayloadSender Sender { get; set; }
    public WebhookPayloadRepository Repository { get; set; }

    public record class WebhookPayloadSender
    {
        public string Login { get; set; }
    }

    public record class WebhookPayloadRepository
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public WebhookPayloadRepositoryOwner Owner { get; set; }

        public record class WebhookPayloadRepositoryOwner
        {
            public int Id { get; set; }
            public string Login { get; set; }
            public string Type { get; set; }
        }
    }
}
