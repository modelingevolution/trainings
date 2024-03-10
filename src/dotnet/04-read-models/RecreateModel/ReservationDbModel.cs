using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
            var r = await Reservations.FirstAsync(x => x.Id == id);
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
                var r = await Reservations.FirstAsync(x => x.Id == id);
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
