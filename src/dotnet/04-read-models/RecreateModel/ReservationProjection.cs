using System.Text.Json;
using EventStore.Client;
using Microsoft.EntityFrameworkCore;
using TrainTicketReservation.Infrastructure;

namespace RecreateModel;

public class ReservationProjection(EventStorePersistentSubscriptionsClient _client)
{
    public const string STREAM_NAME = "$ce-Reservation";
    private static readonly Dictionary<string, Type> _register = new()
    {
        { nameof(ReservationMade), typeof(ReservationMade) },
        { nameof(ReservationOpened), typeof(ReservationOpened) }
    };
    public async Task Start()
    {
        await using var db = new ReservationDbModel();
        await db.RecreateIfTableNamedChanged();

        var result = await _client.ListAllAsync();
        if (!result.Any(x => x.GroupName == ReservationDbModel.RESERVATION_TABLE_NAME && x.EventSource == STREAM_NAME))
            await _client.CreateToStreamAsync(STREAM_NAME, ReservationDbModel.RESERVATION_TABLE_NAME,
                new PersistentSubscriptionSettings(true, StreamPosition.Start));

        var events = _client.SubscribeToStream(STREAM_NAME, ReservationDbModel.RESERVATION_TABLE_NAME);
        await Task.Factory.StartNew(async () => await Subscribe(events), TaskCreationOptions.LongRunning);
    }
   
    private async Task Subscribe(EventStorePersistentSubscriptionsClient.PersistentSubscriptionResult sub)
    {
        await foreach (var e in sub)
        {
            var aggregateId = e.GetAggregateId();
            if (_register.TryGetValue(e.Event.EventType, out var t))
            {
                await using var db = new ReservationDbModel();
                var ev = (IEvent)JsonSerializer.Deserialize(e.Event.Data.Span,t)!;
                await db.Given(aggregateId, ev);
            }
            await sub.Ack(e);
        }
    }
}