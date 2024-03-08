using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using EventStore.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RecreateModel.Contract;
using TrainTicketReservation.Infrastructure;

namespace RecreateModel
{
    internal class ReservationDbModel : DbContext
    {
        public const string VERSION = "v7";
        public const string RESERVATION_TABLE_NAME = $"Reservations_{VERSION}";
        public DbSet<Reservation> Reservations { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer($"Server=.\\;Database=CQRS_04;Trusted_Connection=True;TrustServerCertificate=True;Pooling=true;Max Pool Size=100;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Reservation>().ToTable(RESERVATION_TABLE_NAME);
        }

        public async Task Given(Guid id, IEvent ev)
        {
            switch (ev)
            {
                case ReservationMade e: await Given(id, e); break;
                case ReservationOpened e: await Given(id, e); break;
            }
        }
        private async Task Given(Guid id, ReservationMade ev)
        {
            var t = ev.WindowCount + ev.AisleCount;
            var r = await Reservations.FirstOrDefaultAsync(x => x.Id == id);
            r.ReservedSeatsCount += t;
            r.FreeSeatsCount -= t;
            await SaveChangesAsync();
        }
        private async Task Given(Guid id, ReservationOpened ev)
        {
            var t = ev.AisleCount + ev.WindowCount;
            if (Reservations.Any(x => x.Id == id))
            {
                Console.Error.WriteLine(DateTime.Now.ToShortTimeString() + " We're opening again reservation with the same Id, on production this should not happen");
                var r = await Reservations.FirstOrDefaultAsync(x => x.Id == id);
                r.FreeSeatsCount = t;
                r.TotalSeatsCount = t;
                r.ReservedSeatsCount = 0;
                r.ReservationName = ev.Name;
                await SaveChangesAsync();
                return;
            }
            
            await Reservations.AddAsync(new Reservation() { Id = id, FreeSeatsCount = t, TotalSeatsCount = t, ReservationName = ev.Name ?? "Unkown"});
            await SaveChangesAsync();
        }
    }

    
    public class ReservationProjection(EventStorePersistentSubscriptionsClient _client)
    {
        public const string STREAM_NAME = "$ce-Reservation";
        public async Task Start()
        {
            await using var db = new ReservationDbModel();
            await db.RecreateIfTableNamedChanged();

            var tmp = db.Database.GenerateCreateScript();

            var result = await _client.ListAllAsync();
            if (!result.Any(x => x.GroupName == ReservationDbModel.RESERVATION_TABLE_NAME && x.EventSource == STREAM_NAME))
            {
                await _client.CreateToStreamAsync(STREAM_NAME, ReservationDbModel.RESERVATION_TABLE_NAME,
                    new PersistentSubscriptionSettings(true, StreamPosition.Start, liveBufferSize:0));
            }
            var events = _client.SubscribeToStream(STREAM_NAME, ReservationDbModel.RESERVATION_TABLE_NAME);


            await Task.Factory.StartNew(async () => await Subscribe(events), TaskCreationOptions.LongRunning);

        }
        private async Task Subscribe(EventStorePersistentSubscriptionsClient.PersistentSubscriptionResult sub)
        {
           
            await foreach (var e in sub)
            {
               
                var aggregateId = e.GetAggregateId();
                await using var db = new ReservationDbModel();
                switch (e.Event.EventType)
                {
                    case nameof(ReservationMade):
                        await db.Given(aggregateId, JsonSerializer.Deserialize<ReservationMade>(e.Event.Data.Span)!);
                        break;
                    case nameof(ReservationOpened):
                        await db.Given(aggregateId, JsonSerializer.Deserialize<ReservationOpened>(e.Event.Data.Span)!);
                        break;
                    default: break;
                }

                await sub.Ack(e);
            }
        }
    }
    public class Reservation
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public int FreeSeatsCount { get; set; }
        public int ReservedSeatsCount { get; set; }
        public int TotalSeatsCount { get; set; }
        
        [MaxLength(255)]
        public string? ReservationName { get; set; }
    }
}
