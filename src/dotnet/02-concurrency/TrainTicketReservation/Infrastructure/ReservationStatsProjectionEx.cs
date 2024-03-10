using System.Text.Json;
using EventStore.Client;

namespace TrainTicketReservation.Infrastructure;

public static class StreamSubscriptionResultExtensions
{
    public static async Task WithModel<T>(this EventStoreClient.StreamSubscriptionResult events, T model)
        where T : IModel<T>

    {
        var state = new Tuple<EventStoreClient.StreamSubscriptionResult, T>(events, model);
        await Task.Factory.StartNew(static async (x) =>
        {
            var state = (Tuple<EventStoreClient.StreamSubscriptionResult, T>)x!;
            await foreach (var e in state.Item1)
            {
                if (!T.TypeRegister.TryGetValue(e.Event.EventType, out var t)) continue;

                var aggregateId = Guid.Parse(e.Event.EventStreamId.Substring(e.Event.EventStreamId.IndexOf('-') + 1));
                var ev = JsonSerializer.Deserialize(e.Event.Data.Span, t)!;
                await state.Item2.Given(aggregateId, ev);
            }
        }, state, TaskCreationOptions.LongRunning);
    }
}