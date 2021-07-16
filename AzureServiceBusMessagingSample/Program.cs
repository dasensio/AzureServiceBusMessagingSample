using Azure.Messaging.ServiceBus;
using AzureServiceBusMessagingPoc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace AzureServiceBusMessagingPoc
{
    class Program
    {
        static readonly string _serviceBusConnectionString = "CONNECTION_STRING";

        static async Task Main()
        {
            ConsoleOutputWriter.WriteLine("****************************************************************");
            ConsoleOutputWriter.WriteLine("** Azure ServiceBus Messaging Sample");
            ConsoleOutputWriter.WriteLine("****************************************************************");
            ConsoleOutputWriter.WriteLine();

            await RunAsync();
        }

        static async Task RunAsync()
        {
            Guid applicationId = Guid.NewGuid();

            ServiceBusAdministrationManager managementManager = new ServiceBusAdministrationManager(_serviceBusConnectionString);
            IEnumerable<string> activeGroups = await managementManager.GetActiveTopicNamesAsync();

            if (activeGroups.Any())
            {
                ConsoleOutputWriter.WriteLine("Currently active groups:");
                ConsoleOutputWriter.WriteLines(activeGroups);
                ConsoleOutputWriter.WriteLine();
                ConsoleOutputWriter.WriteLine("Please enter the name of the group you want to join or create:");
            }
            else
            {
                ConsoleOutputWriter.WriteLine("Please enter the name of the group you want to create:");
            }

            string groupName = Console.ReadLine();

            ConsoleOutputWriter.WriteLine();

            ConsoleOutputWriter.WriteLine("Enter your name:");
            string userName = Console.ReadLine();

            ConsoleOutputWriter.WriteLine();

            ConsoleOutputWriter.WriteLine("Initializing the chat group......");

            ConsoleOutputWriter.WriteLine();


            //1.- Create the topic and the subscription for the entered group name and username if they do not already exist 
            await managementManager.CreateSubscriptionIfNotExistsAsync(groupName, userName, TimeSpan.FromMinutes(5));

            IEnumerable<string> activeUsers = await managementManager.GetActiveSubscriptionNamesAsync(groupName);
            if (activeUsers.Any())
            {
                ConsoleOutputWriter.WriteLine("Currently active users:");
                ConsoleOutputWriter.WriteLines(activeUsers);
            }
            else
            {
                ConsoleOutputWriter.WriteLine("There are currently no active users in the group.");
            }

            ConsoleOutputWriter.WriteLine();


            ConversationMessage initialMessage = ConversationMessageBuilder.BuildSystemMessage(applicationId, $"{userName} joined the conversation....", DateTime.Now);

            //2.- Create ServiceBusManager to start sending and processing messages of the chat group 
            await using ServiceBusManager serviceBusManager = new ServiceBusManager(_serviceBusConnectionString, groupName);

            await serviceBusManager.SendMessageAsync(initialMessage);


            //3.- Create ServiceBusProcessor to start processing messages sent to the chat group (topic)
            await using ServiceBusProcessor processor = serviceBusManager.CreateServiceBusProcessor(groupName, userName, MessageHandlerAsync, ErrorHandlerAsync);

            //4.- Beging processing messages
            await processor.StartProcessingAsync();

            while (true)
            {
                string text = Console.ReadLine();
                if (text.Equals("exit", StringComparison.InvariantCultureIgnoreCase))
                {
                    break;
                }

                ConversationMessage userMessage = ConversationMessageBuilder.BuildUserMessage(applicationId, userName, text, DateTime.Now);
                await serviceBusManager.SendMessageAsync(userMessage);
            }

            //5.- Stop processing messages
            await processor.StopProcessingAsync();

            ConversationMessage goodByeMessage = ConversationMessageBuilder.BuildSystemMessage(applicationId, $"{userName} left the conversation....", DateTime.Now);
            await serviceBusManager.SendMessageAsync(goodByeMessage);
        }

        static async Task MessageHandlerAsync(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();

            ConversationMessage message = ConversationMessageBuilder.BuildMessageFromJson(body);
            ConsoleOutputWriter.WriteMessage(message);

            // Complete the message. message is deleted from the subscription queue. 
            await args.CompleteMessageAsync(args.Message);
        }

        static Task ErrorHandlerAsync(ProcessErrorEventArgs args)
        {
            ConsoleOutputWriter.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}