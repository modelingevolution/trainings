using TrainTicketReservation.Infrastructure;

namespace TrainTicketReservation.Reservation.Views.StatView;

public static class MetadataExtensions
{
    public static DateTime Created(this ref Metadata m)
    {
        return m.Data.TryGetProperty("Created", out var e) ? e.GetDateTime() : DateTime.MinValue;
    }
}