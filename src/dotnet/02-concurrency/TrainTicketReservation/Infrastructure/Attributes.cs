namespace TrainTicketReservation.Infrastructure;

[AttributeUsage(AttributeTargets.Class)]
public class EventHandlerAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Class)]
public class AggregateAttribute : Attribute { }