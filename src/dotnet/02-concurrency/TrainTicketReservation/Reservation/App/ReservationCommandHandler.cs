using EventStore.Client;
using TrainTicketReservation.Infrastructure;
using TrainTicketReservation.Reservation.Logic;

namespace TrainTicketReservation.Reservation.App;

public class ReservationCommandHandler(EventStoreClient client) : ICommandHandle<OpenReservation>, ICommandHandle<MakeReservation>
{
    private readonly ReservationStream _stream = new(client);
    public async Task Handle(Guid id, OpenReservation cmd)
    {
        var reservation = ReservationAggregate.Open(id, cmd.Name, cmd.WindowCount, cmd.AisleCount);
        await _stream.New(reservation.Id, reservation.PendingEvents);

    }
    public async Task Handle(Guid id, MakeReservation cmd)
    {
        var reservation = await _stream.Get(id);
        await reservation.Make(cmd.WindowCount, cmd.AisleCount);
        await _stream.Append(reservation.Id, reservation.Age, reservation.PendingEvents);

    }
}