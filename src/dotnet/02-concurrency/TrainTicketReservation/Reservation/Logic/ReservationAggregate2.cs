using TrainTicketReservation.Infrastructure;

namespace TrainTicketReservation.Reservation.Logic;

[Aggregate]
public partial class ReservationAggregate2(Guid id) : AggregateBase<ReservationAggregate2.ReservationState>(id)
{

    public record ReservationState(int FreeWindowSeats, int FreeAisleSeats) { public ReservationState() : this(0,0){} };
    
    public static ReservationAggregate2 Open(Guid id, string name, int w, int a)
    {
        ReservationAggregate2 result = new ReservationAggregate2(id);
        result._pendingEvents.Add(new ReservationOpened(name, w, a));
        return result;
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


    public void Make(int windowCount, int aisleCount)
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