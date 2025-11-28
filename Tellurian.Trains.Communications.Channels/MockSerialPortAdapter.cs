using System.Collections.Concurrent;

namespace Tellurian.Trains.Communications.Channels;

/// <summary>
/// Mock serial port adapter for testing.
/// Allows injecting bytes to be read and capturing bytes that were written.
/// </summary>
public sealed class MockSerialPortAdapter : ISerialPortAdapter
{
    private readonly ConcurrentQueue<byte> _readQueue = new();
    private readonly ConcurrentQueue<byte[]> _writeHistory = new();
    private readonly SemaphoreSlim _readSemaphore = new(0);
    private bool _isOpen;
    private bool _disposed;
    private int _readTimeout = -1; // Infinite by default

    /// <summary>
    /// Creates a new mock serial port adapter.
    /// </summary>
    /// <param name="portName">The simulated port name. Default is "MOCK".</param>
    public MockSerialPortAdapter(string portName = "MOCK")
    {
        PortName = portName;
    }

    /// <inheritdoc />
    public bool IsOpen => _isOpen;

    /// <inheritdoc />
    public string PortName { get; }

    /// <inheritdoc />
    public int ReadTimeout
    {
        get => _readTimeout;
        set => _readTimeout = value;
    }

    /// <summary>
    /// Gets the history of all data written to this mock port.
    /// </summary>
    public IEnumerable<byte[]> WriteHistory => [.. _writeHistory];

    /// <summary>
    /// Gets the total number of write operations.
    /// </summary>
    public int WriteCount => _writeHistory.Count;

    /// <inheritdoc />
    public void Open()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _isOpen = true;
    }

    /// <inheritdoc />
    public void Close()
    {
        _isOpen = false;
    }

    /// <inheritdoc />
    public Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (data is not null && data.Length > 0)
        {
            _writeHistory.Enqueue((byte[])data.Clone());
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async ValueTask<int> ReadByteAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            // Wait for data to be available
            bool acquired;
            if (_readTimeout > 0)
            {
                acquired = await _readSemaphore.WaitAsync(_readTimeout, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await _readSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                acquired = true;
            }

            if (!acquired)
            {
                return -1; // Timeout
            }

            if (_readQueue.TryDequeue(out var b))
            {
                return b;
            }
            return -1;
        }
        catch (OperationCanceledException)
        {
            return -1;
        }
    }

    /// <summary>
    /// Enqueues bytes to be read by the adapter.
    /// </summary>
    /// <param name="data">The bytes to enqueue.</param>
    public void EnqueueBytes(params byte[] data)
    {
        if (data is null) return;
        foreach (var b in data)
        {
            _readQueue.Enqueue(b);
            _readSemaphore.Release();
        }
    }

    /// <summary>
    /// Enqueues a complete LocoNet message (including calculating checksum).
    /// </summary>
    /// <param name="dataWithoutChecksum">Message bytes without checksum.</param>
    public void EnqueueMessageWithChecksum(params byte[] dataWithoutChecksum)
    {
        if (dataWithoutChecksum is null || dataWithoutChecksum.Length == 0) return;

        // Calculate checksum: XOR all bytes, then invert
        byte check = 0;
        foreach (var b in dataWithoutChecksum)
        {
            check ^= b;
        }
        var checksum = (byte)~check;

        EnqueueBytes(dataWithoutChecksum);
        EnqueueBytes(checksum);
    }

    /// <summary>
    /// Clears all enqueued bytes and write history.
    /// </summary>
    public void Clear()
    {
        while (_readQueue.TryDequeue(out _)) { }
        while (_writeHistory.TryDequeue(out _)) { }
        // Reset semaphore
        while (_readSemaphore.CurrentCount > 0)
        {
            _readSemaphore.Wait(0);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _isOpen = false;
            _readSemaphore.Dispose();
            _disposed = true;
        }
    }
}
