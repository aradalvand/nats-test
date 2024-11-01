using System.Diagnostics;
using NATS.Net;
using Shared;

IRequestClient req = new RequestClient();

await using var client = new NatsClient();
while (true)
{
    Console.Write("Enter input: ");
    int input = int.Parse(Console.ReadLine()!);
    Console.WriteLine("Sending request");
    Stopwatch sw = new();
    sw.Start();
    var response = await req.Send(new GenerateOtpRequest(input.ToString()));
    sw.Stop();
    Console.WriteLine($"Response '{response}' received in {sw.Elapsed.TotalMilliseconds:N3} ms");
    Console.WriteLine("----------");
}

public interface IRequestClient
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
        where TResponse : IResponse;
}
public class RequestClient : IRequestClient
{
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default) where TResponse : IResponse
    {
        await using var client = new NatsClient();
        var response = await client.RequestAsync<IRequest<TResponse>, TResponse>(
            request.Subject,
            request,
            cancellationToken: ct
        );
        return response.Data!;
    }
}