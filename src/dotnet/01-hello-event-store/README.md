# Trainings Practices:


## 01-hello-event-store
Sample app that appends messages and receives messages.

To run the app:
```bash
cd src/dotnet/01-hello-event-store
docker-compose up -d
```

In one terminal:
```bash
# Go to Subscriber project:
cd /workspaces/trainings/src/dotnet/01-hello-event-store/Subscriber
dotnet run Fun
```

In another terminal:
```bash
# Go to Send project
cd /workspaces/trainings/src/dotnet/01-hello-event-store/Send
dotnet run Fun
Hello Event Store
```

To cleanup

```bash
cd /workspaces/trainings/src/dotnet/01-hello-event-store
docker-compose down
```

### TODO:

Modify the app:
1) Add Created field in event's metadata.
2) Introduce new event named MessageSend_v2 that contains Sender's Name.
3) Modify the app, so that it has short streams. 

### Questions:

1. What are the drawbacks of this app? Why it is an antipattern.
2. How to modify the app so that it has short streams?