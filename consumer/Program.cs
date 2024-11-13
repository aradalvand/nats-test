using System.Buffers;
using System.Diagnostics;
using System.Text.Json;
using NATS;
using NATS.Client.Core;
using NATS.Client.JetStream.Models;
using NATS.Net;

await using var client = new NatsClient();
var jetStream = client.CreateJetStreamContext();
// jetStream.PublishConcurrentAsync()
// todo: fuck. if partitions are just streams,
var stream = await jetStream.CreateStreamAsync(new(
    "Test:WorkQueue",
    ["Test:WorkQueue.>"]
)
{
    DuplicateWindow = TimeSpan.Zero,
    MaxMsgsPerSubject = 1,
    Discard = StreamConfigDiscard.New,
    DiscardNewPerSubject = true,
    Retention = StreamConfigRetention.Workqueue,
});
var consumer = await stream.CreateOrUpdateConsumerAsync(new("idk")
{
    // NOTE: We want a `MaxDeliver` of `-1` (denoting infinite, so that the message is redelivered until acknowledgement) which is the default, but this is a crucial configuration. See https://docs.nats.io/nats-concepts/jetstream/consumers#:~:text=Yes-,MaxDeliver,-The%20maximum%20number
    // MaxAckPending = -1, // NOTE: Default is 1,000 — setting this to `-1` lifts the restriction, effectively yielding the control flow completely to the client apps — see https://docs.nats.io/nats-concepts/jetstream/consumers#maxackpending
    FilterSubject = $"Test:WorkQueue.>",
});
await foreach (var message in consumer.ConsumeAsync<int>(opts: new()
{
    MaxMsgs = 1,
}))
{
    Console.WriteLine($"Message received: {message.Subject} - {message.Data}");
    Stopwatch sw = new();

    // await Task.Delay(5_000);
    // await message.AckProgressAsync();
    // Console.WriteLine($"Progress acked");
    // await Task.Delay(5_000);

    // CancellationTokenSource cts = new();
    // _ = Task.Run(async () =>
    // {
    //     while (!cts.Token.IsCancellationRequested)
    //     {
    //         Console.WriteLine("ack progress...");
    //         await Task.Delay(5_000, cts.Token);
    //         await message.AckProgressAsync(cancellationToken: cts.Token);
    //     }
    // }, cts.Token);
    // Console.ReadLine();
    // cts.Cancel();

    await Task.Delay(1000);
    sw.Start();
    // NOTE: With `DoubleAck = false` (which is the default) this is effectively fire-and-forget; double-ack ensures that the line after `await AckAsync` would only execute when we have successfully acknowledged the message and it will never be redelivered. See https://docs.nats.io/using-nats/developer/develop_jetstream/model_deep_dive#exactly-once-semantics
    await message.AckAsync(new() { DoubleAck = true });
    sw.Stop();
    Console.WriteLine($"Message acknowledged in {sw.Elapsed.TotalMilliseconds:N3} ms");
    Console.WriteLine("---");
}
