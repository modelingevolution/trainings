namespace TrainTicketReservation.Infrastructure;

public abstract class AggregateBase<TState>(Guid id)
    where TState : new()

{
    protected readonly List<object> _pendingEvents = new();
    protected TState _state = new();
    private long _age = -1;
    public Guid Id => id;
    public long Age => _age;
    public IReadOnlyList<object> PendingEvents => _pendingEvents;
    public async Task Rehydrate(IAsyncEnumerable<object> events)
    {
        await foreach (var e in events)
        {
            Apply(e);
            _age += 1;
        }
    }

    protected abstract void Apply(object ev);
}