﻿using System.Buffers;
using System.Diagnostics;
using System.Text.Json;
using NATS.Client.Core;
using NATS.Net;

await using var client = new NatsClient();
var jetStream = client.CreateJetStreamContext();
while (true)
{
    Stopwatch sw = new();
    Console.Write("Partition Key: ");
    var partitionKey = Console.ReadLine()!;
    sw.Start();
    var foo = new Foo(123, 321);
    // NOTE: The publishing side of nats is extremely simple — no knowledge of specific or anything, no creating "publishers" (a la Pulsar, Kafka) — we simply publish a message to a specific subject. See https://youtu.be/EJJ2SG-cKyM?t=260
    // NOTE: The `PublishAsync` method on the JetStream context is preferable to the one on `NatsClient` because the former gets an acknowledgement from the server. See https://nats-io.github.io/nats.net/documentation/jetstream/publish.html
    var result = await jetStream.PublishAsync($"Shenas.Otps.{partitionKey}", foo, serializer: new Ser());
    sw.Stop();
    Console.WriteLine($"Message sent in {sw.Elapsed.TotalMilliseconds:N3} ms: {JsonSerializer.Serialize(result, options: new() { WriteIndented = true })}");
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