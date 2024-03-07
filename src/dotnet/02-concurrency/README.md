# Trainings Practices:


## 02-Concurrency
Yet another simple console-app ticket reservation system. The app focuses pureply on EventStore and minimal usage of it's APIs. 
IoC integration, aspects injections, REST API, GRPC or UI are purposely omitted. 

To run the app:

1) You can run integration tests.
2) You can launch the app with some arguments

```bash
cd src/dotnet/01-hello-event-store
docker-compose up -d
```

In one terminal:
```bash
# Go to Train Ticket Reservation:
cd /workspaces/trainings/src/dotnet/02-concurrent/TrainTicketReservation
dotnet run open cool_reservation_01 3 2
dotnet run stats
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