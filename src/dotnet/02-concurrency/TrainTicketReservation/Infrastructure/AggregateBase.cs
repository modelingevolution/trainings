namespace TrainTicketReservation.Infrastructure;

public abstract class AggregateBase<TState>(Guid id)
    where TState : new()

{
    private readonly List<object> _pendingEvents = new();
    protected void AppendEvent(object ev)
    {
        _pendingEvents.Add(ev);
        Apply(ev);
    }

    private TState _state = new();
    protected TState State => _state;
    private long _age = -1;
    public Guid Id => id;
    public long Age => _age;
    public IReadOnlyList<object> PendingEvents => _pendingEvents;
    public async Task Rehydrate(IAsyncEnumerable<object> events)
    {
        await foreach (var e in events)
        {
            _state = Apply(e);
            _age += 1;
        }
    }

    protected abstract TState Given(TState state, object ev);
    private TState Apply(object ev) => Given(_state, ev);
}