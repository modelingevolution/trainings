using System.Text.Json;
using EventStore.Client;
using TrainTicketReservation.Infrastructure;
using TrainTicketReservation.Reservation.Logic;

namespace TrainTicketReservation.Reservation.Views.StatView;

[EventHandlerAttribute]
public partial class ReservationStats2
{
    private readonly SortedSet<TimeBucket> _index = new();
    public IReadOnlySet<TimeBucket> Items => _index;
    private async Task Given(Guid id, ReservationMade ev)
    {
        TimeBucket n = ev.When.Date.AddHours(ev.When.TimeOfDay.Hours);

        if (!_index.TryGetValue(n, out var b))
            _index.Add(b = n);

        b.Reserved += ev.AisleCount + ev.WindowCount;
        await Task.Delay(0);
    }

    private async Task Given(Guid id, ReservationOpened ev)
    {
        await Task.Delay(0);
    }
}