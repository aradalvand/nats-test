using System.Buffers;
using System.Diagnostics;
using System.Text.Json;
using NATS.Client.Core;
using NATS.Net;

await using var client = new NatsClient();
while (true)
{
    Stopwatch sw = new();
    Console.Write("Partition Key: ");
    var partitionKey = Console.ReadLine()!;
    sw.Start();
    // NOTE: The publishing side of nats is extremely simple — no knowledge of streams — we simply publish a message to a specific subject. See https://youtu.be/EJJ2SG-cKyM?t=260
    var foo = new Foo(123, 321);
    await client.PublishAsync($"Shenas.Otps.{partitionKey}", foo, serializer: new Ser());
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