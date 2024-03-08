using TrainTicketReservation.Infrastructure;

namespace RecreateModel.Contract;

public record ReservationMade(int WindowCount, int AisleCount) : IEvent
{
    public DateTime When { get; set; } = DateTime.Now;
}