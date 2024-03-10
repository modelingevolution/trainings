using TrainTicketReservation.Infrastructure;

namespace TrainTicketReservation.Reservation.Logic;

public record ReservationOpened(string Name, int WindowCount, int AisleCount) ;