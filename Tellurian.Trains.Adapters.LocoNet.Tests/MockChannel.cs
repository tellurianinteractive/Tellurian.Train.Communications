using Tellurian.Trains.Communications.Channels;
using Tellurian.Trains.Communications.Interfaces.Extensions;

namespace Tellurian.Trains.Adapters.LocoNet.Tests;

/// <summary>
/// Mock implementation of ICommunicationsChannel for testing.
/// </summary>
internal class MockChannel : ICommunicationsChannel
{
    private readonly Observers<CommunicationResult> _observers = new();
    public List<byte[]> SentData { get; } = [];
    public bool ShouldFail { get; set; }
    public int SubscriberCount => _observers.Count;

    public async Task<CommunicationResult> SendAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);

        if (ShouldFail)
        {
            return CommunicationResult.Failure(new InvalidOperationException("Mock failure"));
        }

        SentData.Add(data);
        return CommunicationResult.Success(data, "Mock", "Serial");
    }

    public Task StartReceiveAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public IDisposable Subscribe(IObserver<CommunicationResult> observer)
    {
        return _observers.Subscribe(observer);
    }

    public void SimulateReceive(byte[] data)
    {
        var result = CommunicationResult.Success(data, "Mock", "Serial");
        _observers.Notify(result);
    }

    public void SimulateError(Exception exception)
    {
        _observers.Error(exception);
    }

    public void SimulateCompleted()
    {
        _observers.Completed();
    }
}
