using Azure;
using Azure.Messaging.ServiceBus.Administration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureServiceBusMessagingPoc
{
    public class ServiceBusAdministrationManager
    {
        private readonly string _connectionString;
        private readonly ServiceBusAdministrationClient _client;

        public ServiceBusAdministrationManager(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));

            _connectionString = connectionString;
            _client = new ServiceBusAdministrationClient(_connectionString);
        }

        public async Task CreateSubscriptionIfNotExistsAsync(string topicName, string userName, TimeSpan? autoDeleteOnIdle = null)
        {
            if (string.IsNullOrWhiteSpace(topicName)) throw new ArgumentNullException(nameof(topicName));
            if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentNullException(nameof(userName));

            await CreateTopicIfnotExistsAsync(topicName);

            bool subscriptionExists = await _client.SubscriptionExistsAsync(topicName, userName);
            if (!subscriptionExists)
            {
                await CreateSubscriptionAsync(topicName, userName, autoDeleteOnIdle);
            }
        }

        public async Task<IEnumerable<string>> GetActiveTopicNamesAsync()
        {
            AsyncPageable<TopicProperties> topics = _client.GetTopicsAsync();

            HashSet<string> activeTopicNames = new HashSet<string>();
            await foreach (TopicProperties topic in topics)
            {
                if (topic.Status == EntityStatus.Active)
                {
                    activeTopicNames.Add(topic.Name);
                }
            }

            return activeTopicNames;
        }

        public async Task<IEnumerable<string>> GetActiveSubscriptionNamesAsync(string topicName)
        {
            AsyncPageable<SubscriptionProperties> subscriptions = _client.GetSubscriptionsAsync(topicName);

            HashSet<string> activeTopicNames = new HashSet<string>();
            await foreach (SubscriptionProperties subscription in subscriptions)
            {
                if (subscription.Status == EntityStatus.Active)
                {
                    activeTopicNames.Add(subscription.SubscriptionName);
                }
            }

            return activeTopicNames;
        }

        async Task CreateTopicIfnotExistsAsync(string topicName)
        {
            bool topicExists = await _client.TopicExistsAsync(topicName);

            if (topicExists) return;

            await _client.CreateTopicAsync(topicName);
        }

        async Task CreateSubscriptionAsync(string topicName, string userName, TimeSpan? autoDeleteOnIdle = null)
        {
            CreateSubscriptionOptions description = new CreateSubscriptionOptions(topicName, userName)
            {
                AutoDeleteOnIdle = autoDeleteOnIdle ?? TimeSpan.MaxValue
            };

            await _client.CreateSubscriptionAsync(description);
        }
    }
}
