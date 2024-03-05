# Trainings Practices:


## 04-Read-Models
Here we have 2 apps:

* First app shows how to recreate a new read-model without affecting the previous one.
* Second shows how to combine uniqueness constraints into a command-handler.

To run the app:
```bash
cd src/dotnet/04-read-models
docker-compose up -d
```

To cleanup

```bash
cd /workspaces/trainings/src/dotnet/04-read-models
docker-compose down
```

### Exercises:

Modify the first app:
1. Remove a column from read-model.

Modify the second app:
1. Change the rule, so that names of meetups must be unique per year. 

### Questions:
1. Discuss canary deployment and blue-green deployment.