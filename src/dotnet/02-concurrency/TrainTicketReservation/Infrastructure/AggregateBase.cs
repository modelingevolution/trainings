﻿namespace TrainTicketReservation.Infrastructure;

public abstract class AggregateBase<TState>(Guid id)
    where TState : new()

{
    private readonly List<object> _pendingEvents = new();

    protected TState State { get; private set; } = new();

    public Guid Id => id;
    public long Age { get; private set; } = -1;

    public IReadOnlyList<object> PendingEvents => _pendingEvents;

    protected void AppendEvent(object ev)
    {
        _pendingEvents.Add(ev);
        Apply(ev);
    }

    public async Task Rehydrate(IAsyncEnumerable<object> events)
    {
        await foreach (var e in events)
        {
            State = Apply(e);
            Age += 1;
        }
    }

    protected abstract TState Given(TState state, object ev);

    private TState Apply(object ev)
    {
        return Given(State, ev);
    }
}