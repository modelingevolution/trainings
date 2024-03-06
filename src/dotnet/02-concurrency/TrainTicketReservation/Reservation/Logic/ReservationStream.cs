using System.Text.Json;
using EventStore.Client;
using TrainTicketReservation.Infrastructure;

namespace TrainTicketReservation.Reservation.Logic;

public class ReservationStream(EventStoreClient client)
{
    public async Task<ReservationAggregate> New(Guid id)
    {
        return new ReservationAggregate(id);
    }
    public IAsyncEnumerable<IEvent> ReadEvents(Guid id)
    {
        var items = client.ReadStreamAsync(Direction.Forwards, $"Reservation-{id}", StreamPosition.Start);
        var events = items.Select(ev => ev.Event.EventType switch
        {
            nameof(ReservationMade) => JsonSerializer.Deserialize<ReservationMade>(ev.Event.Data.Span),
            nameof(ReservationOpened) => (IEvent)JsonSerializer.Deserialize<ReservationOpened>(ev.Event.Data.Span),
            _ => throw new InvalidOperationException()
        });
        return events;
    }

    public async Task Append(Guid id, long age, IEnumerable<IEvent> events)
    {
        var evData = events.Select(x => x switch
        {
            ReservationMade e => new EventData(Uuid.NewUuid(), nameof(ReservationMade), JsonSerializer.SerializeToUtf8Bytes(e)),
        });
        await client.AppendToStreamAsync($"Reservation-{id}", StreamRevision.FromInt64(age), evData);
    }

    public async Task New(Guid id, IEnumerable<IEvent> events)
    {
        var evData = events.Select(x => x switch
        {
            ReservationOpened e => new EventData(Uuid.NewUuid(), nameof(ReservationOpened), JsonSerializer.SerializeToUtf8Bytes(e))
        });
        await client.AppendToStreamAsync($"Reservation-{id}", StreamState.NoStream, evData);
    }


}