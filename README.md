# Trainings Practices:


## 01-hello-event-store
Sample app that sends and receives messages.

To run the app:
```bash
docker-compose up -d
```

In one terminal:
```bash
# Go to Subscriber project:
cd src/dotnet/01-hello-event-store/Subscriber
dotnet run Fun
```

In another terminal:
```bash
# Go to Send project
cd src/dotnet/01-hello-event-store/Send
dotnet run Fun
Hello Event Store
```

