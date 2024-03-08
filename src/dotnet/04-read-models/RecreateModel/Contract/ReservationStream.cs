using System.Text.Json;
using EventStore.Client;
using TrainTicketReservation.Infrastructure;

namespace RecreateModel.Contract;

public class ReservationStream(EventStoreClient client)
{
    private static HashSet<string> SupportedEventTypes = new HashSet<string>() { nameof(ReservationMade) , nameof(ReservationOpened) };
    public IAsyncEnumerable<IEvent> ReadEvents(Guid id)
    {
        var items = client.ReadStreamAsync(Direction.Forwards, $"Reservation-{id}", StreamPosition.Start);
        var events = items
            .Where(x=>SupportedEventTypes.Contains(x.Event.EventType))
            .Select(ev => ev.Event.EventType switch
        {
            nameof(ReservationMade) => JsonSerializer.Deserialize<ReservationMade>(ev.Event.Data.Span),
            nameof(ReservationOpened) => (IEvent)JsonSerializer.Deserialize<ReservationOpened>(ev.Event.Data.Span)!,
            _ => throw new InvalidOperationException()
        });
        return events;
    }

    public async Task Append(Guid id, long age, IEnumerable<IEvent> events)
    {
        var evData = events.Select(x => new EventData(Uuid.NewUuid(), x.GetType().Name, JsonSerializer.SerializeToUtf8Bytes(x)));

        await client.AppendToStreamAsync($"Reservation-{id}", StreamRevision.FromInt64(age), evData);
    }

    public async Task New(Guid id, IEnumerable<IEvent> events)
    {
        var evData = events.Select(x => new EventData(Uuid.NewUuid(), x.GetType().Name, JsonSerializer.SerializeToUtf8Bytes(x)));

        await client.AppendToStreamAsync($"Reservation-{id}", StreamState.NoStream, evData);
    }


}