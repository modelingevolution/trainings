using EventStore.Client;
using TrainTicketReservation.Infrastructure;
using TrainTicketReservation.Reservation.Logic;

namespace TrainTicketReservation.Reservation.App;

public class ReservationCommandHandler(EventStoreClient client) 
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
        reservation.Make(cmd.WindowCount, cmd.AisleCount);

        await _stream.Append(reservation.Id, reservation.Age, reservation.PendingEvents);

    }
}
public class ReservationCommandHandler2(EventStoreClient client)
{
    public async Task Handle(Guid id, OpenReservation cmd)
    {
        var reservation = ReservationAggregate2.Open(id, cmd.Name, cmd.WindowCount, cmd.AisleCount);
        await client.SaveNew(reservation, (aggregate,ev) => new { Created=DateTime.Now});
    }
    public async Task Handle(Guid id, MakeReservation cmd)
    {
        var reservation = await client.Get<ReservationAggregate2>(id);
        reservation.Make(cmd.WindowCount, cmd.AisleCount);
        await client.SaveChanges(reservation);

    }
}