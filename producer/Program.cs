using System.Buffers;
using System.Diagnostics;
using System.Text.Json;
using NATS.Client.Core;
using NATS.Net;
using Shared;

var handler = new GenerateOtpRequestHandler();

await using var client = new NatsClient();
Console.WriteLine("Waiting for requests");
await foreach (var request in client.SubscribeAsync<GenerateOtpRequest>(nameof(GenerateOtpRequest))) // todo: we should get it from 'Subject'
{
    Stopwatch sw = new();
    sw.Start();
    var response = await handler.Handle(request.Data!);
    await request.ReplyAsync(response);
    sw.Stop();
    Console.WriteLine($"Replied '{response}' in {sw.Elapsed.TotalMilliseconds:N3} ms");
    Console.WriteLine("----------");
}

public class GenerateOtpRequestHandler : IRequestHandler<GenerateOtpRequest, GenerateOtpReply>
{
    public async Task<GenerateOtpReply> Handle(GenerateOtpRequest input, CancellationToken ct = default)
    {
        Console.WriteLine($"Incoming request '{input}' — replying");
        return new(new Random().Next(1, 9));
    }
}