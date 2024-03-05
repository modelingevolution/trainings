# Trainings Practices:


## 03-projections
In this example you will learn how to create js projection from UI and from Code.
The projection shall aggregate different events into one stream as links.

To run the app:
```bash
cd src/dotnet/03-projections
docker-compose up -d
```

In one terminal:
```bash
# Go to Train Ticket Reservation:
cd /workspaces/trainings/src/dotnet/03-projections/Cli
dotnet run create
dotnet run append ProjectDefined
dotnet run append TaskDefined
dotnet run append ProjectClosed
```

To cleanup

```bash
cd /workspaces/trainings/src/dotnet/03-projections
docker-compose down
```

### Exercises:

Modify the app:
1. Modify the projection so that new type of event will be processed.

### Questions:
