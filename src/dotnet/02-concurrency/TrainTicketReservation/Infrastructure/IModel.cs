using EventStore.Client;
using System.Text.Json;

namespace TrainTicketReservation.Infrastructure;

public interface IModel<TSelf>
{
    static abstract IDictionary<string, Type> TypeRegister { get; }
    Task Given(Guid id, object ev);
}
public interface IAggregate<out TSelf>
{
    static abstract IDictionary<string, Type> TypeRegister { get; }
    static abstract TSelf New(Guid id);
    Guid Id { get; }
    long Age { get; }
    IReadOnlyList<object> PendingEvents { get; }
    Task Rehydrate(IAsyncEnumerable<object> events);
}
public static class EventStoreClientExtensions
{
    public static async Task<T> Get<T>(this EventStoreClient client, Guid id)
        where T : IAggregate<T>
    {
        var items = client.ReadStreamAsync(Direction.Forwards, $"{typeof(T).Name}-{id}", StreamPosition.Start);
        var registry = T.TypeRegister;
        var events = items.Select(ev => JsonSerializer.Deserialize(ev.Event.Data.Span, registry[ev.Event.EventType])!);

        var aggregate = T.New(id);
        await aggregate.Rehydrate(events);
        return aggregate;
    }

    public static async Task SaveChanges<T>(this EventStoreClient client, T aggregate)
    where T : IAggregate<T>
    {
        string streamId = $"{typeof(T).Name}-{aggregate.Id}";
        var evData = aggregate.PendingEvents.Select(x => new EventData(Uuid.NewUuid(), x.GetType().Name, JsonSerializer.SerializeToUtf8Bytes(x)));
        await client.AppendToStreamAsync(streamId, StreamRevision.FromInt64(aggregate.Age), evData);
    }

    public static async Task SaveNew<T>(this EventStoreClient client, T aggregate)
        where T : IAggregate<T>
    {
        string streamId = $"{typeof(T).Name}-{aggregate.Id}";
        var evData = aggregate.PendingEvents.Select(x => new EventData(Uuid.NewUuid(), x.GetType().Name, JsonSerializer.SerializeToUtf8Bytes(x)));
        await client.AppendToStreamAsync(streamId, StreamState.NoStream, evData);
    }
}
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