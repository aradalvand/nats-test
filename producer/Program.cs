using System.Buffers;
using System.Diagnostics;
using System.Text.Json;
using NATS.Client.Core;
using NATS.Net;

await using var client = new NatsClient();
Console.WriteLine("Waiting for requests");
await foreach (var request in client.SubscribeAsync<int>("request.*"))
{
    var response = new Random().Next(1, 100);
    Console.WriteLine("Incoming request — replying");
    Stopwatch sw = new();
    sw.Start();
    await request.ReplyAsync(response);
    sw.Stop();
    Console.WriteLine($"Replied '{response}' in {sw.Elapsed.TotalMilliseconds:N3} ms");
    Console.WriteLine("----------");
}
