using Microsoft.Extensions.Logging;

namespace Tellurian.Trains.Communications.Channels;

/// <summary>
/// Communication channel for LocoNet over UDP (multicast).
/// Supports both loconetd (Glenn Butcher) and GCA101 (Rocrail/Peter Giling) configurations.
/// Raw binary LocoNet messages are sent/received as UDP datagrams — no text framing or headers.
/// </summary>
public sealed class UdpLocoNetChannel : ICommunicationsChannel, IAsyncDisposable, IDisposable
{
    private readonly IUdpLocoNetAdapter _adapter;
    private readonly Observers<CommunicationResult> _observers = new();
    private readonly ILogger _logger;
    private readonly bool _validateChecksum;
    private Task? _receiveTask;
    private bool _disposed;

    /// <summary>
    /// Creates a new UDP LocoNet channel.
    /// </summary>
    /// <param name="adapter">The UDP adapter to use for multicast communication.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="validateChecksum">Whether to validate LocoNet checksums on received datagrams.</param>
    public UdpLocoNetChannel(IUdpLocoNetAdapter adapter, ILogger<UdpLocoNetChannel> logger, bool validateChecksum = true)
    {
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _validateChecksum = validateChecksum;
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("UdpLocoNetChannel created for {Endpoint}", adapter.EndpointName);
    }

    /// <inheritdoc />
    public async Task<CommunicationResult> SendAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        if (data is null || data.Length == 0) return CommunicationResult.NoOperation();
        try
        {
            if (!_adapter.IsJoined)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning("UDP adapter for {Endpoint} was not joined, joining now", _adapter.EndpointName);
                await _adapter.JoinAsync(cancellationToken).ConfigureAwait(false);
            }
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("UDP send: {Data}", BitConverter.ToString(data));
            await _adapter.SendAsync(data, cancellationToken).ConfigureAwait(false);
            return CommunicationResult.Success(data, _adapter.EndpointName, "LocoNetUDP");
        }
        catch (Exception ex)
        {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError(ex, "UDP send failed to {Endpoint}", _adapter.EndpointName);
            return CommunicationResult.Failure(ex);
        }
    }

    /// <inheritdoc />
    public IDisposable Subscribe(IObserver<CommunicationResult> observer)
    {
        return _observers.Subscribe(observer);
    }

    /// <inheritdoc />
    public async Task StartReceiveAsync(CancellationToken cancellationToken = default)
    {
        if (!_adapter.IsJoined)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("UDP adapter for {Endpoint} was not joined, joining now", _adapter.EndpointName);
            await _adapter.JoinAsync(cancellationToken).ConfigureAwait(false);
        }
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("UDP receive task starting for {Endpoint}", _adapter.EndpointName);
        _receiveTask = ReceiveLoopAsync(cancellationToken);
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var data = await _adapter.ReceiveAsync(cancellationToken).ConfigureAwait(false);
                if (data is null)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.LogInformation("UDP adapter closed for {Endpoint}", _adapter.EndpointName);
                    break;
                }
                if (data.Length < 2)
                {
                    if (_logger.IsEnabled(LogLevel.Warning))
                        _logger.LogWarning("UDP received datagram too short ({Length} bytes) from {Endpoint}", data.Length, _adapter.EndpointName);
                    continue;
                }
                if (_validateChecksum && !IsValidLocoNetMessage(data))
                {
                    if (_logger.IsEnabled(LogLevel.Warning))
                        _logger.LogWarning("UDP received invalid LocoNet message from {Endpoint}: {Data}", _adapter.EndpointName, BitConverter.ToString(data));
                    continue;
                }
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("UDP receive: {Data}", BitConverter.ToString(data));
                _observers.Notify(CommunicationResult.Success(data, _adapter.EndpointName, "LocoNetUDP"));
            }
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation, exit silently
        }
        catch (ObjectDisposedException)
        {
            // Adapter was disposed, exit silently
        }
        catch (Exception ex)
        {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError(ex, "UDP receive failed from {Endpoint}", _adapter.EndpointName);
            _observers.Error(ex);
        }
    }

    /// <summary>
    /// Validates that the byte array is a valid LocoNet message.
    /// A valid message has at least 2 bytes, the first byte has MSB=1 (valid opcode),
    /// and the XOR of all bytes equals 0xFF (valid checksum).
    /// </summary>
    internal static bool IsValidLocoNetMessage(byte[] data)
    {
        if (data is null || data.Length < 2) return false;
        if ((data[0] & 0x80) == 0) return false;
        byte xor = 0;
        foreach (var b in data)
        {
            xor ^= b;
        }
        return xor == 0xFF;
    }

    private async Task CloseAsync()
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("UdpLocoNetChannel closing asynchronously for {Endpoint}", _adapter.EndpointName);
        _adapter.Close();
        if (_receiveTask is not null)
        {
            try
            {
                await _receiveTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
        }
        _observers.Completed();
    }

    private void CloseSync()
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("UdpLocoNetChannel closing synchronously for {Endpoint}", _adapter.EndpointName);
        _adapter.Close();
        _observers.Completed();
    }

    #region IDisposable and IAsyncDisposable Support

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await CloseAsync().ConfigureAwait(false);
            _adapter.Dispose();
            _disposed = true;
        }
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                CloseSync();
                _adapter.Dispose();
            }
            _disposed = true;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
