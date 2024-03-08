using EventStore.Client;
using JoinEvents.Infrastructure;

namespace JoinEvents
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string outputStream = args.GetRequiredArgumentFor("-i");
            string fromStreamsArg = string.Join(',', args.GetUnnamedArguments().Select(x => $"'$et-{x}'"));

            string query = $"fromStreams([{fromStreamsArg}]).when( {{ " +
                           $"\n    $any : function(s,e) {{ linkTo('{outputStream}', e) }}" +
                           $"\n}});";

            var client = CreateClient();
            if (await client.ListAllAsync().AnyAsync(x => x.Name == outputStream))
            {
                var state = await client.GetStatusAsync(outputStream);
                if (state.Status != "Stopped")
                    await client.DisableAsync(outputStream);
                await client.UpdateAsync(outputStream, query);
                await client.EnableAsync(outputStream);
            }
            else
            {
                
                await client.CreateContinuousAsync(outputStream, query, true);
            }
            Console.WriteLine("Done");

        }
        private static EventStoreProjectionManagementClient CreateClient()
        {
            const string connectionString = "esdb://admin:changeit@localhost:2113?tls=false&tlsVerifyCert=false";

            var settings = EventStoreClientSettings.Create(connectionString);

            var client = new EventStoreProjectionManagementClient(settings);
            return client;
        }
    }
}
