# Trainings Practices:


## 02-Concurrency
Yet another simple ticket reservation system.

To run the app:
```bash
cd src/dotnet/01-hello-event-store
docker-compose up -d
```

In one terminal:
```bash
# Go to Train Ticket Reservation:
cd /workspaces/trainings/src/dotnet/02-concurrent/TrainTicketReservation
dotnet run open cool_reservation_01 3 2
```

In another terminal:
```bash
# Make reservation
dotnet run reserve cool_reservation_01 window
dotnet run reserve cool_reservation_01 window
dotnet run reserve cool_reservation_01 window

# Next one shall fail.
dotnet run reserve cool_reservation_01 window
```

To cleanup

```bash
cd /workspaces/trainings/src/dotnet/02-concurrent
docker-compose down
```

### Exercises:

Modify the app:
1. Add a rule, that single person cannot reserve more than 5 spots.

### Questions:
1. Train 007 departures from London every day at 7:00. 
2. What aggregate do we have? Train / Reservation? If reservation, what is reservation? How to model reservation?