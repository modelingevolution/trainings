using EventStore.Client;
using TrainTicketReservation.Reservation.App;
using TrainTicketReservation.Reservation.Views.StatView;

namespace TrainTicketReservation.Infrastructure;

public static class HandlerFactory
{
    public static ReservationCommandHandler CreateReservationCommandHandler()
    {
        var client = CreateClient();

        return new ReservationCommandHandler(client);
    }

    public static ReservationStatsProjection CreateReservationStatsEventHandler()
    {
        var client = CreateClient();

        return new ReservationStatsProjection(client);
    }

    private static EventStoreClient CreateClient()
    {
        const string connectionString = "esdb://admin:changeit@localhost:2113?tls=false&tlsVerifyCert=false";

        var settings = EventStoreClientSettings.Create(connectionString);

        var client = new EventStoreClient(settings);
        return client;
    }
}