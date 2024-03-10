using EventStore.Client;
using TrainTicketReservation.Reservation.App;
using TrainTicketReservation.Reservation.Views.StatView;

namespace TrainTicketReservation;

public class App : IDisposable, IAsyncDisposable
{
    private EventStoreClient? _client;
    private static App? _instance;

    public static App Instance => _instance ??= new App();

    public ReservationCommandHandler CreateReservationCommandHandler()
    {
        _client ??= CreateClient();

        return new ReservationCommandHandler(_client);
    }
    public ReservationCommandHandler2 CreateReservationCommandHandler2()
    {
        _client ??= CreateClient();

        return new ReservationCommandHandler2(_client);
    }
    public ReservationStatsProjection CreateReservationStatsEventHandler()
    {
        _client ??= CreateClient();

        return new ReservationStatsProjection(_client);
    }

    public static EventStoreClient CreateClient()
    {
        const string connectionString = "esdb://admin:changeit@localhost:2113?tls=false&tlsVerifyCert=false";

        var settings = EventStoreClientSettings.Create(connectionString);

        var client = new EventStoreClient(settings);
        return client;
    }

    public void Dispose()
    {
        _client?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_client != null) await _client.DisposeAsync();
    }
}