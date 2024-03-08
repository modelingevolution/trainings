using TrainTicketReservation.Infrastructure;

namespace RecreateModel.Contract;

public record ReservationOpened(string Name, int WindowCount, int AisleCount) : IEvent;