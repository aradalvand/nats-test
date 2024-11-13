using System.Buffers;
using System.Diagnostics;
using System.Text.Json;
using NATS.Client.Core;
using NATS.Net;
using Shared;

await using var client = new NatsClient();
var jetStream = client.CreateJetStreamContext();
while (true)
{
    Stopwatch sw = new();
    Console.Write("Message ID: ");
    var messageId = Console.ReadLine()!;
    sw.Start();
    var result = await jetStream.PublishAsync($"Test:WorkQueue.OtpGeneratedMessageConsumer.{messageId}", 123);
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
