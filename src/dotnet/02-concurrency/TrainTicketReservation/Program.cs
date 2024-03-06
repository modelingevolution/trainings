using System.Text.Json;
using EventStore.Client;

namespace TrainTicketReservation
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var arg0 = args[0];
                switch (arg0)
                {
                    case "open":
                        await HandlerFactory.CreateReservationCommandHandler().Handle(arg0.ToGuid(), new OpenReservation(arg0, int.Parse(args[1]), int.Parse(args[2])));
                        break;
                    case "reserve":
                        var tt = Enum.Parse<TicketType>(args[1]);
                        await HandlerFactory.CreateReservationCommandHandler().Handle(arg0.ToGuid(),
                            new MakeReservation(tt == TicketType.Window ? 1 : 0, tt == TicketType.Aisle ? 1 : 0));
                        break;

                }
            }
            catch (Exception ex)
            {
                Console.Error.Write(ex.Message);
            }
        }

       
    }

    public static class HandlerFactory
    {
        public static ReservationCommandHandler CreateReservationCommandHandler()
        {
            const string connectionString = "esdb://admin:changeit@localhost:2113?tls=false&tlsVerifyCert=false";

            var settings = EventStoreClientSettings.Create(connectionString);

            var client = new EventStoreClient(settings);

            return new ReservationCommandHandler(client);
        }
    }

    public enum TicketType
    {
        Aisle =1, Window =2
    }
    public interface IEvent {}
    public record OpenReservation(string Name, int WindowCount, int AisleCount);
    public record ReservationOpened(string Name, int WindowCount, int AisleCount) : IEvent;
    public record MakeReservation(int WindowCount, int AisleCount);
    public record ReservationMade(int WindowCount, int AisleCount) : IEvent;
    public class ReservationAggregate(Guid _id)
    {
        private readonly List<IEvent> _pendingEvents = new();
        private ReservationState _state = new(0,0);
        private long _age=-1;
        public Guid Id => _id;
        public void Commited() => _pendingEvents.Clear();
        public record ReservationState(int FreeWindowSeats, int FreeAisleSeats);

        public IReadOnlyList<IEvent> PendingEvents => _pendingEvents;
        public long Age => _age;

        public static ReservationAggregate Open(Guid id, string name, int w, int a)
        {
            ReservationAggregate result = new ReservationAggregate(id);
            result._pendingEvents.Add(new ReservationOpened(name,w,a));
            return result;
        }
        public async Task<ReservationAggregate> Rehydrate(IAsyncEnumerable<IEvent> events)
        {
            await foreach (var e in events) {
                Apply(e);
                _age += 1;
            }

            return this;
        }

        private void Apply(IEvent e)
        {
            _state = e switch
            {
                ReservationOpened ro => Given(_state, ro),
                ReservationMade rm => Given(_state, rm),
                _ => throw new InvalidOperationException()
            };
        }

        private static ReservationState Given(ReservationState state, ReservationOpened ev)
        {
            return state with
            {
                FreeAisleSeats = ev.AisleCount, 
                FreeWindowSeats = ev.WindowCount
            };
        }
        private static ReservationState Given(ReservationState state, ReservationMade ev)
        {
            return state with
            {
                FreeAisleSeats = state.FreeAisleSeats - ev.AisleCount,
                FreeWindowSeats = state.FreeWindowSeats - ev.WindowCount
            };
        }
        

        public async Task Make(int windowCount, int aisleCount)
        {
            if (_state.FreeWindowSeats >= windowCount && _state.FreeAisleSeats >= aisleCount)
            {
                var ev = new ReservationMade(windowCount, aisleCount);
                _pendingEvents.Add(ev);
                Apply(ev);
            }
            else throw new SeatsUnavailable();
        }

    }
    public class SeatsUnavailable : Exception{}
    public static class ReservationStreamExtensions
    {
        public static async Task<ReservationAggregate> Get(this ReservationStream stream, Guid id)
        {
            return await new ReservationAggregate(id).Rehydrate(stream.ReadEvents(id));
        }
    }
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
            var evData = events.Select(x=> x switch
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
    public class ReservationCommandHandler(EventStoreClient client) : ICommandHandle<OpenReservation>, ICommandHandle<MakeReservation>
    {
        private readonly ReservationStream _stream = new (client);
        public async Task Handle(Guid id, OpenReservation cmd)
        {
            var reservation = ReservationAggregate.Open(id, cmd.Name, cmd.WindowCount, cmd.AisleCount);
            await _stream.New(reservation.Id, reservation.PendingEvents);
            
        }
        public async Task Handle(Guid id, MakeReservation cmd)
        {
            var reservation = await _stream.Get(id);
            await reservation.Make(cmd.WindowCount, cmd.AisleCount);
            await _stream.Append(reservation.Id, reservation.Age, reservation.PendingEvents);
            
        }
    }
}
