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
    "TEST",
    ["Shenas.Otps.>"]
)
{
    DuplicateWindow = TimeSpan.Zero,
    MaxMsgsPerSubject = 1,
    Discard = StreamConfigDiscard.New,
    DiscardNewPerSubject = true,
});
var consumer = await stream.CreateOrUpdateConsumerAsync(new("WhyShouldINameTheConsumer"));
await foreach (var message in consumer.ConsumeAsync(new Ser()))
{
    Console.WriteLine($"Message received: {message.Subject} - {message.Data}");
    Stopwatch sw = new();
    sw.Start();
    // NOTE: With `DoubleAck = false` (which is the default) this is effectively fire-and-forget; double-ack ensures that the line after `await AckAsync` would only execute when we have successfully acknowledged the message and it will never be redelivered. See https://docs.nats.io/using-nats/developer/develop_jetstream/model_deep_dive#exactly-once-semantics
    await message.AckAsync(new() { DoubleAck = true });
    sw.Stop();
    Console.WriteLine($"Message acknowledged in {sw.Elapsed.TotalMilliseconds:N3} ms");
    Console.WriteLine("-    --");
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