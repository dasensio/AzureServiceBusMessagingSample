using System;

namespace AzureServiceBusMessagingPoc.Model
{
    public class ConversationMessage
    {
        public const string SYSTEM_USER_NAME = "System";

        public Guid ApplicationId { get; set; }
        public string User { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }

        public ConversationMessage() { }

        public ConversationMessage(Guid applicationId, string user, string text, DateTime date)
        {
            ApplicationId = applicationId;
            User = user;
            Text = text;
            Date = date;
        }
    }
}
