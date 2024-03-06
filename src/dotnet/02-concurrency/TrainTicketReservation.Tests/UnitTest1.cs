using System.Data;
using FluentAssertions;

namespace TrainTicketReservation.Tests
{
    public class ReservationIntegrationTests
    {
        private string TrainReservationNo = "Train_01" + DateTime.Now.ToString();
        [Fact]
        public async Task SeatsUnavailable()
        {
            var id = TrainReservationNo.ToGuid();
            await HandlerFactory.CreateReservationCommandHandler().Handle(id, new OpenReservation(TrainReservationNo, 2,1));
            await HandlerFactory.CreateReservationCommandHandler().Handle(id, new MakeReservation(1, 0));
            await HandlerFactory.CreateReservationCommandHandler().Handle(id, new MakeReservation(1, 1));


            Func<Task> action = async () => await HandlerFactory.CreateReservationCommandHandler().Handle(id, new MakeReservation(1, 0));
            
            await action.Should().ThrowAsync<SeatsUnavailable>();
        }
    }
}