using System.Diagnostics;
using NATS;
using NATS.Net;

await using var client = new NatsClient();
while (true)
{
    Stopwatch sw = new();
    Console.Write("Partition Key: ");
    var partitionKey = Console.ReadLine()!;
    sw.Start();
    await client.PublishAsync($"Shenas.Otps.{partitionKey}", new Random().Next(1, 9));
    sw.Stop();
    Console.WriteLine($"Message sent in {sw.Elapsed.TotalMilliseconds:N3} ms");
    Console.WriteLine("---");

    Console.CancelKeyPress += async (e, b) =>
    {
        Console.WriteLine("Disposing client and producer.");
        await client.DisposeAsync();
        Console.WriteLine("Disposed client and producer.");
    };
}