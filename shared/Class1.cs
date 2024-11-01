namespace Shared;

public interface IResponse;
public interface IRequest<out TResponse> where TResponse : IResponse
{
    string Subject { get; }
}

public record GenerateOtpRequest(
    string PhoneNumber
) : IRequest<GenerateOtpReply>
{
    public string Subject => nameof(GenerateOtpRequest);
}

public record GenerateOtpReply(
    int Result
) : IResponse;

public interface IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResponse
{
    public Task<TResponse> Handle(TRequest input, CancellationToken ct = default);
}