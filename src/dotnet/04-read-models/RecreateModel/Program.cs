using EventStore.Client;

namespace RecreateModel
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var projection = new ReservationProjection(CreateClient());
            await projection.Start();
            Console.WriteLine("Started");
            Console.WriteLine("Press a key to exit.");
            Console.ReadLine();
        }
        private static EventStorePersistentSubscriptionsClient CreateClient()
        {
            const string connectionString = "esdb://admin:changeit@localhost:2113?tls=false&tlsVerifyCert=false";

            var settings = EventStoreClientSettings.Create(connectionString);

            var client = new EventStorePersistentSubscriptionsClient(settings);
            return client;
        }
    }
}