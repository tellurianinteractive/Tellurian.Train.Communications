using Microsoft.Extensions.Logging;

namespace Tellurian.Trains.Communications.Channels;

/// <summary>
/// Communication channel using a serial port with protocol-specific message framing.
/// </summary>
public sealed class SerialDataChannel : ICommunicationsChannel, IAsyncDisposable, IDisposable
{
    private readonly ISerialPortAdapter _serialPort;
    private readonly IByteStreamFramer _framer;
    private readonly Observers<CommunicationResult> _observers = new();
    private readonly ILogger _logger;
    private Task? _receiveTask;
    private bool _disposed;

    /// <summary>
    /// Creates a new serial data channel.
    /// </summary>
    /// <param name="serialPort">The serial port adapter to use.</param>
    /// <param name="framer">The message framer for the protocol being used.</param>
    /// <param name="logger">The logger instance.</param>
    public SerialDataChannel(ISerialPortAdapter serialPort, IByteStreamFramer framer, ILogger<SerialDataChannel> logger)
    {
        _serialPort = serialPort ?? throw new ArgumentNullException(nameof(serialPort));
        _framer = framer ?? throw new ArgumentNullException(nameof(framer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("SerialDataChannel created for port {PortName} using {ProtocolName} protocol", serialPort.PortName, framer.ProtocolName);
    }

    /// <inheritdoc />
    public async Task<CommunicationResult> SendAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        if (data is null || data.Length == 0) return CommunicationResult.NoOperation();
        try
        {
            if (!_serialPort.IsOpen)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning("Serial port {PortName} was not open, opening now", _serialPort.PortName);
                _serialPort.Open();
            }
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Serial send: {Data}", BitConverter.ToString(data));
            await _serialPort.WriteAsync(data, cancellationToken).ConfigureAwait(false);
            return CommunicationResult.Success(data, _serialPort.PortName, _framer.ProtocolName);
        }
        catch (Exception ex)
        {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError(ex, "Serial send failed on port {PortName}", _serialPort.PortName);
            return CommunicationResult.Failure(ex);
        }
    }

    /// <inheritdoc />
    public IDisposable Subscribe(IObserver<CommunicationResult> observer)
    {
        return _observers.Subscribe(observer);
    }

    /// <inheritdoc />
    public Task StartReceiveAsync(CancellationToken cancellationToken = default)
    {
        if (!_serialPort.IsOpen)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("Serial port {PortName} was not open, opening now", _serialPort.PortName);
            _serialPort.Open();
        }
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Serial receive task starting on port {PortName}", _serialPort.PortName);
        _receiveTask = ReceiveAsync(cancellationToken);
        return Task.CompletedTask;
    }

    private async Task ReceiveAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = await _framer.ReadMessageAsync(
                    _serialPort.ReadByteAsync,
                    cancellationToken).ConfigureAwait(false);

                if (message is not null)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug("Serial receive: {Data}", BitConverter.ToString(message));
                    _observers.Notify(CommunicationResult.Success(
                        message,
                        _serialPort.PortName,
                        _framer.ProtocolName));
                }
                // If message is null, the framer timed out or is resyncing - continue the loop
            }
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation, exit silently
        }
        catch (ObjectDisposedException)
        {
            // Port was disposed, exit silently
        }
        catch (Exception ex)
        {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError(ex, "Serial receive failed on port {PortName}", _serialPort.PortName);
            _observers.Error(ex);
        }
    }

    private async Task CloseAsync()
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("SerialDataChannel closing asynchronously on port {PortName}", _serialPort.PortName);
        _serialPort.Close();
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
            _logger.LogInformation("SerialDataChannel closing synchronously on port {PortName}", _serialPort.PortName);
        _serialPort.Close();
        _observers.Completed();
    }

    #region IDisposable and IAsyncDisposable Support

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await CloseAsync().ConfigureAwait(false);
            _serialPort.Dispose();
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
                _serialPort.Dispose();
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
