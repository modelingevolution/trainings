using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EventStore.Client;
using Microsoft.VisualBasic.CompilerServices;
using TrainTicketReservation.Infrastructure;
using TrainTicketReservation.Reservation.Logic;

namespace TrainTicketReservation.Reservation.Views.StatView
{
    public class ReservationStatsProjection(EventStoreClient client)
    {
        public readonly ReservationStats View = new ReservationStats();
        
        public async Task Start()
        {
            var events = client.SubscribeToStream("$ce-Reservation", FromStream.Start, true);
            await Task.Factory.StartNew(async () => await Subscribe(events), TaskCreationOptions.LongRunning);
        }

        public event Action? Changed;
        private async Task Subscribe(EventStoreClient.StreamSubscriptionResult events)
        {
            await foreach (var e in events)
            {
                var aggregateId = Guid.Parse(e.Event.EventStreamId.Substring(e.Event.EventStreamId.IndexOf('-') + 1));
                switch (e.Event.EventType)
                {
                    case nameof(ReservationMade):
                        View.Given(aggregateId, JsonSerializer.Deserialize<ReservationMade>(e.Event.Data.Span) ?? throw new Exception("Deserialization failed"));
                        Changed?.Invoke();
                        break;
                    case nameof(ReservationOpened):
                        break;
                    default: break;
                }
            }
        }
    }
    public class ReservationStats
    {
        public class TimeBucket(DateTime When) : IComparable<TimeBucket>, IComparable
        {
            private readonly DateTime _when = When;
            public DateTime When => _when;
            public int Reserved { get; set; }
            public static implicit operator TimeBucket(DateTime t) => new TimeBucket(t);
            public int CompareTo(TimeBucket? other)
            {
                if (ReferenceEquals(this, other)) return 0;
                if (ReferenceEquals(null, other)) return 1;
                return _when.CompareTo(other._when);
            }

            public int CompareTo(object? obj)
            {
                if (ReferenceEquals(null, obj)) return 1;
                if (ReferenceEquals(this, obj)) return 0;
                return obj is TimeBucket other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(TimeBucket)}");
            }
        };
        
        private readonly SortedSet<TimeBucket> _index = new();
        
        public IReadOnlySet<TimeBucket> Items => _index;

        
        public void Given(Guid id, IEvent ev)
        {
            switch (ev)
            {
                case ReservationMade e: Given(id, e);
                    break;
            }
        }
        

        private void Given(Guid id, ReservationMade ev)
        {
            TimeBucket n = ev.When.Date.AddHours(ev.When.TimeOfDay.Hours);

            if (!_index.TryGetValue(n, out var b)) 
                _index.Add(b = n);

            b.Reserved += ev.AisleCount + ev.WindowCount;
        }
    }
}
