const { EventStoreDBClient, jsonEvent } = require('@eventstore/db-client');
const crypto = require('crypto');
const { v4: uuidv4 } = require('uuid');
const readline = require('readline');

// Convert string to Guid
function toGuid(input) {
  if (!input) {
    throw new Error('Input is null or empty');
  }
  const hash = crypto.createHash('sha256').update(input).digest();
  return uuidv4({ random: hash.slice(0, 16) });
}

// Create EventStore client
function createClient() {
  const connectionString = 'esdb://admin:changeit@localhost:2113?tls=false';
  const client = EventStoreDBClient.connectionString(connectionString);
  return client;
}

class MessageSent {
  constructor(text) {
    this.id = uuidv4();
    this.text = text;
  }
}

async function main(args) {
  if (args.length === 0) {
    console.error('Expected topic name.');
    return;
  }

  const topicId = toGuid(args[0]);
  const client = createClient();
  const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
  });

  for await (const line of rl) {
    if (line === 'exit') {
      rl.close();
      break;
    }

    const evt = new MessageSent(line);
    const eventData = jsonEvent({ type: 'MessageSent', data: evt });

    await client.appendToStream(`Thread-${topicId}`, eventData);

    console.log('Event appended');
  }
}

// Node.js does not have a default argument object similar to args in C#, 
// hence we pass the process.argv, which contains runtime arguments
main(process.argv.slice(2));
