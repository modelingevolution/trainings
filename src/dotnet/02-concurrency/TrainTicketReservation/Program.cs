﻿using TrainTicketReservation.Infrastructure;
using TrainTicketReservation.Reservation;
using TrainTicketReservation.Reservation.App;
using TrainTicketReservation.Reservation.Logic;
using TrainTicketReservation.Reservation.Views.StatView;

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
                        await App.Instance.CreateReservationCommandHandler().Handle(arg0.ToGuid(), new OpenReservation(arg0, int.Parse(args[1]), int.Parse(args[2])));
                        break;
                    case "reserve":
                        var tt = Enum.Parse<TicketType>(args[1]);
                        await App.Instance.CreateReservationCommandHandler().Handle(arg0.ToGuid(),
                            new MakeReservation(tt == TicketType.Window ? 1 : 0, tt == TicketType.Aisle ? 1 : 0));
                        break;
                    case "stats":
                        var handler = App.Instance.CreateReservationStatsEventHandler();
                        await handler.Start();
                        await Task.Delay(1000);
                        handler.Changed += () => Print(handler.View);
                        Print(handler.View);
                        break;
                }
                Console.WriteLine("Success");
            }
            catch (Exception ex)
            {
                Console.Error.Write(ex.Message);
            }

            if (args.Any(x => x == "-i"))
            {
                Console.WriteLine("Hit a key to close the app.");
                Console.ReadLine();
            }
        }

        private static void Print(ReservationStats stats)
        {
            foreach (var i in stats.Items)
            {
                Console.WriteLine($"{i.When}:\t{i.Reserved}");
            }
        }
    }
}
