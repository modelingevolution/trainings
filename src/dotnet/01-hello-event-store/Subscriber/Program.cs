using System.Text.Json;
using Contracts;
using EventStore.Client;
using Send;

namespace Subscriber
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Expected topic name.");
                return;
            }

            var topicId = args[0].ToGuid();
            var client = CreateClient();
            await using var events = client.SubscribeToStream($"Thread-{topicId}", FromStream.End, true);
            await foreach (var i in events)
            {
                switch (i.Event.EventType)
                {
                    case nameof(MessageSent):
                        var ev = JsonSerializer.Deserialize<MessageSent>(i.Event.Data.Span);
                        Console.WriteLine($"{DateTime.Now}: {ev.Text}");
                        break;
                }
                
            }
        }
        static EventStoreClient CreateClient()
        {
            const string connectionString = "esdb://admin:changeit@localhost:2113?tls=false&tlsVerifyCert=false";

            var settings = EventStoreClientSettings.Create(connectionString);

            var client = new EventStoreClient(settings);
            return client;
        }
    }

}
