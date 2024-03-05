using EventStore.Client;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Contracts;

namespace Send
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
            string line = "";

            var client = CreateClient();

            while (line != "exit")
            {
                line = Console.ReadLine();

                var evt = new MessageSent { Text = line };

                var eventData = new EventData(
                    Uuid.NewUuid(),
                    nameof(MessageSent),
                    JsonSerializer.SerializeToUtf8Bytes(evt)
                );

                await client.AppendToStreamAsync(
                    $"Thread-{topicId}",
                    StreamState.Any,
                    new[] { eventData }
                );
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
