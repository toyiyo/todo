namespace toyiyo.todo.Notifications.Jobs
{
    public class EmailSendingArgs
    {
        public long UserId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public int? TenantId { get; internal set; }
    }
}
