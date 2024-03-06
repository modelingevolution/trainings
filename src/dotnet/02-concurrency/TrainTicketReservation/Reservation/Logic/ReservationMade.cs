using TrainTicketReservation.Infrastructure;

namespace TrainTicketReservation.Reservation.Logic;

public record ReservationMade(int WindowCount, int AisleCount) : IEvent
{
    public DateTime When { get; set; } = DateTime.Now;
}