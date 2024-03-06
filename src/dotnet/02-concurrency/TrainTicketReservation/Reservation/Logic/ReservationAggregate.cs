using TrainTicketReservation.Infrastructure;

namespace TrainTicketReservation.Reservation.Logic;

public class ReservationAggregate(Guid _id)
{
    private readonly List<IEvent> _pendingEvents = new();
    private ReservationState _state = new(0, 0);
    private long _age = -1;
    public Guid Id => _id;
    public void Commited() => _pendingEvents.Clear();
    public record ReservationState(int FreeWindowSeats, int FreeAisleSeats);

    public IReadOnlyList<IEvent> PendingEvents => _pendingEvents;
    public long Age => _age;

    public static ReservationAggregate Open(Guid id, string name, int w, int a)
    {
        ReservationAggregate result = new ReservationAggregate(id);
        result._pendingEvents.Add(new ReservationOpened(name, w, a));
        return result;
    }
    public async Task<ReservationAggregate> Rehydrate(IAsyncEnumerable<IEvent> events)
    {
        await foreach (var e in events)
        {
            Apply(e);
            _age += 1;
        }

        return this;
    }

    private void Apply(IEvent e)
    {
        _state = e switch
        {
            ReservationOpened ro => Given(_state, ro),
            ReservationMade rm => Given(_state, rm),
            _ => throw new InvalidOperationException()
        };
    }

    private static ReservationState Given(ReservationState state, ReservationOpened ev)
    {
        return state with
        {
            FreeAisleSeats = ev.AisleCount,
            FreeWindowSeats = ev.WindowCount
        };
    }
    private static ReservationState Given(ReservationState state, ReservationMade ev)
    {
        return state with
        {
            FreeAisleSeats = state.FreeAisleSeats - ev.AisleCount,
            FreeWindowSeats = state.FreeWindowSeats - ev.WindowCount
        };
    }


    public async Task Make(int windowCount, int aisleCount)
    {
        if (_state.FreeWindowSeats >= windowCount && _state.FreeAisleSeats >= aisleCount)
        {
            var ev = new ReservationMade(windowCount, aisleCount);
            _pendingEvents.Add(ev);
            Apply(ev);
        }
        else throw new SeatsUnavailable();
    }

}