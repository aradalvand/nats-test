using NATS;
using NATS.Net;

await using var client = new NatsClient();
await foreach (var message in client.SubscribeAsync<int>("Shenas.Otps.*"))
{
    Console.WriteLine($"Message received: {message.Data}");
}

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
