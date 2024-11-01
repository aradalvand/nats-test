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
    bool Result
) : IResponse;