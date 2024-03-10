using System.Text.Json;

namespace TrainTicketReservation.Infrastructure;

public interface IReadModel<TSelf>
{
    static abstract IDictionary<string, Type> TypeRegister { get; }
    Task Given(Metadata m, object ev);
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

public readonly struct Metadata(Guid id, JsonElement data)
{
    public Guid Id => id;
    public JsonElement Data => data;
}
