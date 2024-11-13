using System.Buffers;
using System.Diagnostics;
using System.Text.Json;
using NATS;
using NATS.Client.Core;
using NATS.Client.JetStream.Models;
using NATS.Net;

await using var client = new NatsClient();
var jetStream = client.CreateJetStreamContext(); var stream = await jetStream.CreateStreamAsync(new(
    "Shenas:Workqueue",
    ["Shenas:Workqueue.>"]
)
{
    DuplicateWindow = TimeSpan.Zero,
    MaxMsgsPerSubject = 1,
    Discard = StreamConfigDiscard.New,
    DiscardNewPerSubject = true,
    Retention = StreamConfigRetention.Workqueue,
    AllowDirect = true, // NOTE: Allows us to actually see the stream contents from outside the application
});
_ = Watch("1");
_ = Watch("2");
Console.ReadLine();

// jetStream.PublishConcurrentAsync()

async Task Watch(string id)
{
    var consumer = await stream.CreateOrUpdateConsumerAsync(new("idk") // todo: we might not really need a durable consumer here at all
    {
        FilterSubject = "Shenas:Workqueue.OtpGeneratedMessageConsumer2.>",
    });
    await foreach (var message in consumer.ConsumeAsync<string>())
    {
        Console.WriteLine($"({id}) Message received: {message.Subject} - {message.Data}");
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
        Console.WriteLine($"({id}) Message acknowledged in {sw.Elapsed.TotalMilliseconds:N3} ms");
        Console.WriteLine("---");
    }
}
