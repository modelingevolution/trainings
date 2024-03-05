const { EventStoreDBClient, jsonEvent, START, END } = require("@eventstore/db-client");
const crypto = require("crypto");
const { v4: uuidv4 } = require("uuid");

// Convert string to Guid
function toGuid(input) {
  if (!input) {
    throw new Error("Input is null or empty");
  }
  const hash = crypto.createHash("sha256").update(input).digest();
  return uuidv4({ random: hash.slice(0, 16) });
}

// Create EventStore client
function createClient() {
  const connectionString = "esdb://admin:changeit@localhost:2113?tls=false";
  return EventStoreDBClient.connectionString(connectionString);
}

class MessageSent {
  constructor({ id = uuidv4(), text }) {
    this.id = id;
    this.text = text;
  }
}

async function main(args) {
  if (args.length === 0) {
    console.error("Expected topic name.");
    return;
  }

  const topicId = toGuid(args[0]);
  const client = createClient();
  
  const events = client.subscribeToStream(`Thread-${topicId}`, {
    fromRevision: END,
    resolveLinkTos: true,
  });

  for await (const { event } of events) {
    if (event?.type === "MessageSent") {
       console.log(`${new Date().toISOString()}: ${event.data.text}`);
    }
  }
}

// Node.js does not have a default argument object similar to args in C#, 
// hence we pass the process.argv, which contains runtime arguments
main(process.argv.slice(2));
