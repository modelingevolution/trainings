using TrainTicketReservation.Infrastructure;

namespace RecreateModel;

public record ReservationMade(int WindowCount, int AisleCount) : IEvent
{
    public DateTime When { get; set; } = DateTime.Now;
}
public record ReservationOpened(string Name, int WindowCount, int AisleCount) : IEvent;
