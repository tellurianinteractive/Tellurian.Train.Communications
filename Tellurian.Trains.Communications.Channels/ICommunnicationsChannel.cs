using System.Globalization;

namespace Tellurian.Trains.Communications.Channels;

public interface ICommunicationsChannel : IObservable<CommunicationResult>
{
    Task<CommunicationResult> SendAsync(byte[] data, CancellationToken cancellationToken = default);
    Task StartReceiveAsync(CancellationToken cancellationToken = default);
}

public abstract class CommunicationResult
{
    protected CommunicationResult()
    {
        Timestamp = DateTimeOffset.Now;
    }
    public static CommunicationResult Success(byte[] data, string remoteEndpointName, string protocolName) =>
        new SuccessResult(data, remoteEndpointName, protocolName);
    public static CommunicationResult Failure(Exception ex) => new FailureResult(ex);
    public static CommunicationResult NoOperation() => new NoOperationResult();
    public virtual bool IsSuccess { get; } = false;
    public virtual int Length { get; } = 0;
    public DateTimeOffset Timestamp { get; }
}
public sealed class SuccessResult : CommunicationResult
{
    internal SuccessResult(byte[] data, string remoteEndpointName, string protocolName)
    {
        _Data = data;
        RemoteEndpointName = remoteEndpointName;
        ProtocolName = protocolName;
    }
    private readonly byte[] _Data;
    public override bool IsSuccess => true;
    public override int Length => _Data.Length;
    public byte[] Data() => _Data;
    public string RemoteEndpointName { get; }
    public string ProtocolName { get; }
    public override string ToString() => string.Format(CultureInfo.InvariantCulture, "SUCCESS: Remote endpoint: {0}, Protocol: {1}, Data: {2}", RemoteEndpointName, ProtocolName, BitConverter.ToString(_Data));
}

public sealed class FailureResult : CommunicationResult
{
    internal FailureResult(Exception ex)
    {
        Exception = ex;
    }
    public Exception Exception { get; }
    public override string ToString() => string.Format(CultureInfo.InvariantCulture, "FAILURE: Message: {0}", Exception.Message);
}

public sealed class NoOperationResult : CommunicationResult
{
    public override string ToString() => "NONE: No operation performed.";
}
