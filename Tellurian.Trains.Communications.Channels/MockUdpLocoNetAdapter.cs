using System.Collections.Concurrent;

namespace Tellurian.Trains.Communications.Channels;

/// <summary>
/// Mock UDP adapter for testing.
/// Allows injecting datagrams to be received and capturing datagrams that were sent.
/// </summary>
public sealed class MockUdpLocoNetAdapter : IUdpLocoNetAdapter
{
    private readonly ConcurrentQueue<byte[]?> _receiveQueue = new();
    private readonly ConcurrentQueue<byte[]> _sendHistory = new();
    private readonly SemaphoreSlim _receiveSemaphore = new(0);
    private bool _isJoined;
    private bool _disposed;

    /// <inheritdoc />
    public bool IsJoined => _isJoined;

    /// <inheritdoc />
    public string EndpointName { get; } = "MOCK:4501";

    /// <summary>
    /// Gets the history of all datagrams sent through this mock adapter.
    /// </summary>
    public IEnumerable<byte[]> SentData => [.. _sendHistory];

    /// <summary>
    /// Gets the total number of send operations.
    /// </summary>
    public int SendCount => _sendHistory.Count;

    /// <inheritdoc />
    public Task JoinAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _isJoined = true;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<byte[]?> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        try
        {
            await _receiveSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (_receiveQueue.TryDequeue(out var data))
            {
                return data;
            }
            return null;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public Task SendAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _sendHistory.Enqueue(data);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Enqueues a datagram to be returned by the next <see cref="ReceiveAsync"/> call.
    /// </summary>
    /// <param name="data">The datagram bytes to enqueue.</param>
    public void EnqueueDatagram(byte[] data)
    {
        _receiveQueue.Enqueue(data);
        _receiveSemaphore.Release();
    }

    /// <summary>
    /// Simulates connection closed by enqueuing a null datagram.
    /// </summary>
    public void SimulateClose()
    {
        _receiveQueue.Enqueue(null);
        _receiveSemaphore.Release();
    }

    /// <inheritdoc />
    public void Close()
    {
        _isJoined = false;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _isJoined = false;
            _receiveSemaphore.Dispose();
            _disposed = true;
        }
    }
}
