using Newtonsoft.Json;
using System;

namespace AzureServiceBusMessagingPoc.Model
{
    public static class ConversationMessageBuilder
    {
        public static ConversationMessage BuildUserMessage(Guid applicationId, string user, string message, DateTime date)
        {
            return new ConversationMessage(applicationId, user, message, date);
        }
        public static ConversationMessage BuildSystemMessage(Guid applicationId, string message, DateTime date)
        {
            return new ConversationMessage(applicationId, ConversationMessage.SYSTEM_USER_NAME, message, date);
        }
        public static ConversationMessage BuildMessageFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            return JsonConvert.DeserializeObject<ConversationMessage>(json);
        }
    }
}
