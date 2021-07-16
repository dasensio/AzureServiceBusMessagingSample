using Azure.Messaging.ServiceBus;
using System;
using System.Threading.Tasks;

namespace AzureServiceBusMessagingPoc
{
    public class ServiceBusManager : IAsyncDisposable
    {
        private readonly string _connectionString;
        private readonly string _queueOrTopicName;
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;
        private readonly ServiceBusReceiver _receiver;

        public ServiceBusManager(string connectionString, string queueOrTopicName)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            if (string.IsNullOrWhiteSpace(queueOrTopicName)) throw new ArgumentNullException(nameof(queueOrTopicName));

            _connectionString = connectionString;
            _queueOrTopicName = queueOrTopicName;
            _client = new ServiceBusClient(_connectionString);
            _sender = _client.CreateSender(_queueOrTopicName);
            _receiver = _client.CreateReceiver(_queueOrTopicName);
        }

        public async Task SendMessageAsync<TMessage>(TMessage message) where TMessage : class
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            string body = Newtonsoft.Json.JsonConvert.SerializeObject(message);

            ServiceBusMessage sbMessage = new ServiceBusMessage(body);
            await _sender.SendMessageAsync(sbMessage);
        }

        public async Task<TMessage> ReceiveMessageAsync<TMessage>() where TMessage : class
        {
            ServiceBusReceivedMessage receivedMessage = await _receiver.ReceiveMessageAsync();

            string body = receivedMessage.Body.ToString();
            TMessage message = Newtonsoft.Json.JsonConvert.DeserializeObject<TMessage>(body);
            return message;
        }

        public ServiceBusProcessor CreateServiceBusProcessor(string topicName, string subscriptionName, Func<ProcessMessageEventArgs, Task> onProcessMessageAsync, Func<ProcessErrorEventArgs, Task> onProcessErrorAsync)
        {
            if (string.IsNullOrWhiteSpace(topicName)) throw new ArgumentNullException(nameof(topicName));
            if (string.IsNullOrWhiteSpace(subscriptionName)) throw new ArgumentNullException(nameof(subscriptionName));
            if (onProcessMessageAsync == null) throw new ArgumentNullException(nameof(onProcessMessageAsync));
            if (onProcessErrorAsync == null) throw new ArgumentNullException(nameof(onProcessErrorAsync));

            ServiceBusProcessor processor = _client.CreateProcessor(topicName, subscriptionName, new ServiceBusProcessorOptions());
            processor.ProcessMessageAsync += onProcessMessageAsync;
            processor.ProcessErrorAsync += onProcessErrorAsync;

            return processor;
        }

        public async ValueTask DisposeAsync()
        {
            await _receiver.DisposeAsync();
            await _sender.DisposeAsync();
            await _client.DisposeAsync();
        }
    }
}