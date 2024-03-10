namespace TrainTicketReservation.Reservation.Logic;

public static class ReservationStreamExtensions
{
    public static async Task<ReservationAggregate> Get(this ReservationStream stream, Guid id)
    {
        return await new ReservationAggregate(id)
            .Rehydrate(stream.ReadEvents(id));
    }
}