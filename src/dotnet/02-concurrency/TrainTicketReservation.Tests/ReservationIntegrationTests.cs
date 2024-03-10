using System.Data;
using EventStore.Client;
using FluentAssertions;
using TrainTicketReservation.Infrastructure;
using TrainTicketReservation.Reservation.App;
using TrainTicketReservation.Reservation.Logic;

namespace TrainTicketReservation.Tests
{
    public class ReservationIntegrationTests : IDisposable, IAsyncDisposable
    {
        private string TrainReservationNo = "Train_01" + DateTime.Now.ToString();
        private readonly App _app = new();

       

        [Fact]
        public async Task MakingReservationForUnavailableSeats_Throws_SeatsUnavailable()
        {
            var id = TrainReservationNo.ToGuid();
            await _app.CreateReservationCommandHandler().Handle(id, new OpenReservation(TrainReservationNo, 2,1));
            await _app.CreateReservationCommandHandler().Handle(id, new MakeReservation(1, 0));
            await _app.CreateReservationCommandHandler().Handle(id, new MakeReservation(1, 1));


            Func<Task> action = async () => await _app.CreateReservationCommandHandler().Handle(id, new MakeReservation(1, 0));
            
            await action.Should().ThrowAsync<SeatsUnavailable>();
        }

        [Fact]
        public async Task MakingReservation2ForUnavailableSeats_Throws_SeatsUnavailable()
        {
            var id = TrainReservationNo.ToGuid();
            await _app.CreateReservationCommandHandler2().Handle(id, new OpenReservation(TrainReservationNo, 2, 1));
            await _app.CreateReservationCommandHandler2().Handle(id, new MakeReservation(1, 0));
            await _app.CreateReservationCommandHandler2().Handle(id, new MakeReservation(1, 1));


            Func<Task> action = async () => await _app.CreateReservationCommandHandler2().Handle(id, new MakeReservation(1, 0));

            await action.Should().ThrowAsync<SeatsUnavailable>();
        }

        [Fact]
        public async Task ConcurrentCommandHandlerInvocation_Throws_WrongExpectedVersionException()
        {
            var id = TrainReservationNo.ToGuid();

            await _app.CreateReservationCommandHandler().Handle(id, new OpenReservation(TrainReservationNo, 2000, 2000));

            // Let's spin 2 threads that want to make some reservations. Let's simulate little delay, so that we'd give a chance to finish processing in parallel some amount of items.
            async Task Reserve()
            {
                await using var app = new App();
                var handler = app.CreateReservationCommandHandler();
                for (int i = 0; i < 1000; i++)
                {
                    await Task.Delay(200+Random.Shared.Next(300));
                    await handler.Handle(id, new MakeReservation(1, 1));
                }
            }

            Func<Task> action = async () => await(await Task.WhenAny(Reserve(), Reserve()));

            //await action();
            await action.Should().ThrowAsync<WrongExpectedVersionException>();
        }

        public void Dispose()
        {
            _app?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await _app.DisposeAsync();
        }
    }
}