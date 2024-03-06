namespace TrainTicketReservation.Infrastructure;

public interface ICommandHandle<in TCommand>
{
    Task Handle(Guid id, TCommand cmd);
}

