using System.Collections.Concurrent;

namespace Tellurian.Trains.Communications.Channels;

/// <summary>
/// Mock TCP stream adapter for testing.
/// Allows injecting lines to be read and capturing lines that were written.
/// </summary>
public sealed class MockTcpStreamAdapter : ITcpStreamAdapter
{
    private readonly ConcurrentQueue<string?> _readQueue = new();
    private readonly ConcurrentQueue<string> _writeHistory = new();
    private readonly SemaphoreSlim _readSemaphore = new(0);
    private bool _isConnected;
    private bool _disposed;

    /// <inheritdoc />
    public bool IsConnected => _isConnected;

    /// <inheritdoc />
    public string RemoteEndpointName { get; } = "MOCK:1234";

    /// <summary>
    /// Gets the history of all lines written to this mock stream.
    /// </summary>
    public IEnumerable<string> WriteHistory => [.. _writeHistory];

    /// <summary>
    /// Gets the total number of write operations.
    /// </summary>
    public int WriteCount => _writeHistory.Count;

    /// <inheritdoc />
    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _isConnected = true;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<string?> ReadLineAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        try
        {
            await _readSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (_readQueue.TryDequeue(out var line))
            {
                return line;
            }
            return null;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public Task WriteLineAsync(string line, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _writeHistory.Enqueue(line);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Enqueues a line to be returned by the next <see cref="ReadLineAsync"/> call.
    /// </summary>
    /// <param name="line">The line to enqueue.</param>
    public void EnqueueLine(string line)
    {
        _readQueue.Enqueue(line);
        _readSemaphore.Release();
    }

    /// <summary>
    /// Simulates a server disconnect by enqueuing a null line.
    /// </summary>
    public void SimulateDisconnect()
    {
        _readQueue.Enqueue(null);
        _readSemaphore.Release();
    }

    /// <inheritdoc />
    public void Close()
    {
        _isConnected = false;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _isConnected = false;
            _readSemaphore.Dispose();
            _disposed = true;
        }
    }
}
