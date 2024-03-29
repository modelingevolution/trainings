﻿using System.Text.Json;
using EventStore.Client;
using TrainTicketReservation.Infrastructure;

namespace TrainTicketReservation.Reservation.Logic;

public class ReservationStream(EventStoreClient client)
{
   public IAsyncEnumerable<object> ReadEvents(Guid id)
    {
        var items = client.ReadStreamAsync(Direction.Forwards, $"Reservation-{id}", StreamPosition.Start);
        IAsyncEnumerable<object> events = items.Select(ev => ev.Event.EventType switch
        {
            nameof(ReservationMade) => JsonSerializer.Deserialize<ReservationMade>(ev.Event.Data.Span),
            nameof(ReservationOpened) => (object)JsonSerializer.Deserialize<ReservationOpened>(ev.Event.Data.Span)!,
            _ => throw new InvalidOperationException()
        })!;
        return events;
    }

   private static readonly Dictionary<string, Type> _register = new()
   {
       { nameof(ReservationMade), typeof(ReservationMade) }, 
       { nameof(ReservationOpened), typeof(ReservationOpened) }
   };
   public IAsyncEnumerable<object> ReadEvents_Reflection(Guid id)
   {
       var items = client.ReadStreamAsync(Direction.Forwards, $"Reservation-{id}", StreamPosition.Start);
       var events = items.Select(ev => (object)JsonSerializer.Deserialize(ev.Event.Data.Span, _register[ev.Event.EventType])!);
       return events;
   }

    public async Task Append(Guid id, long age, IEnumerable<object> events)
   {
       var evData = events.Select(x => new EventData(Uuid.NewUuid(), x.GetType().Name, JsonSerializer.SerializeToUtf8Bytes(x)));

       await client.AppendToStreamAsync($"Reservation-{id}", StreamRevision.FromInt64(age), evData);
   }

   public async Task New(Guid id, IEnumerable<object> events)
   {
       var evData = events.Select(x => new EventData(Uuid.NewUuid(), x.GetType().Name, JsonSerializer.SerializeToUtf8Bytes(x)));

       await client.AppendToStreamAsync($"Reservation-{id}", StreamState.NoStream, evData);
   }


}