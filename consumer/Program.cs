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
    "Shenas:WorkQueue",
    ["Shenas:WorkQueue.>"]
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
});
await foreach (var message in consumer.ConsumeAsync<int>(opts: new()
{
    MaxMsgs = 1,

}))
{
    Console.WriteLine($"Message received: {message.Subject} - {message.Data}");
    Stopwatch sw = new();
    // NOTE: With `DoubleAck = false` (which is the default) this is effectively fire-and-forget; double-ack ensures that the line after `await AckAsync` would only execute when we have successfully acknowledged the message and it will never be redelivered. See https://docs.nats.io/using-nats/developer/develop_jetstream/model_deep_dive#exactly-once-semantics

    await Task.Delay(10_000);

    // CancellationTokenSource cts = new();
    // _ = Task.Run(async () =>
    // {
    //     while (!cts.Token.IsCancellationRequested)
    //     {
    //         await Task.Delay(1000, cts.Token);
    //         await message.AckProgressAsync(cancellationToken: cts.Token);
    //         Console.WriteLine("Ack progress");
    //     }
    // });
    // Console.Write("Press enter to ack");
    // Console.ReadLine();
    // cts.Cancel();

    sw.Start();
    await message.AckAsync(new() { DoubleAck = true });
    sw.Stop();
    Console.WriteLine($"Message acknowledged in {sw.Elapsed.TotalMilliseconds:N3} ms");
    Console.WriteLine("---");
}

public class Ser : INatsSerializer<Foo>
{
    public INatsSerializer<Foo> CombineWith(INatsSerializer<Foo> next)
    {
        throw new NotImplementedException();
    }

    public Foo? Deserialize(in ReadOnlySequence<byte> buffer)
    {
        // todo: inefficient
        var value = JsonSerializer.Deserialize<Foo>(buffer.ToArray());
        return value;
    }

    public void Serialize(IBufferWriter<byte> bufferWriter, Foo value)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
        bufferWriter.Write(bytes);
    }
}

public record Foo(
    int X,
    int Y
);