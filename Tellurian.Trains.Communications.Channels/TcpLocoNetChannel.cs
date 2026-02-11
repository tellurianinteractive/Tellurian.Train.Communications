using System.Globalization;
using Microsoft.Extensions.Logging;

namespace Tellurian.Trains.Communications.Channels;

/// <summary>
/// Communication channel implementing the LoconetOverTcp protocol (by Stefan Bormann).
/// Bridges LocoNet messages over a TCP connection using ASCII-encoded hex lines.
/// </summary>
public sealed class TcpLocoNetChannel : ICommunicationsChannel, IAsyncDisposable, IDisposable
{
    private readonly ITcpStreamAdapter _stream;
    private readonly Observers<CommunicationResult> _observers = new();
    private readonly ILogger _logger;
    private Task? _receiveTask;
    private bool _disposed;

    /// <summary>
    /// Creates a new TCP LocoNet channel.
    /// </summary>
    /// <param name="stream">The TCP stream adapter to use.</param>
    /// <param name="logger">The logger instance.</param>
    public TcpLocoNetChannel(ITcpStreamAdapter stream, ILogger<TcpLocoNetChannel> logger)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("TcpLocoNetChannel created for {RemoteEndpoint}", stream.RemoteEndpointName);
    }

    /// <summary>
    /// Gets the server version string received from the server, if any.
    /// </summary>
    public string? ServerVersion { get; private set; }

    /// <inheritdoc />
    public async Task<CommunicationResult> SendAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        if (data is null || data.Length == 0) return CommunicationResult.NoOperation();
        try
        {
            if (!_stream.IsConnected)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning("TCP stream to {RemoteEndpoint} was not connected, connecting now", _stream.RemoteEndpointName);
                await _stream.ConnectAsync(cancellationToken).ConfigureAwait(false);
            }
            var hex = FormatHex(data);
            var line = "SEND " + hex;
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("TCP send: {Line}", line);
            await _stream.WriteLineAsync(line, cancellationToken).ConfigureAwait(false);
            return CommunicationResult.Success(data, _stream.RemoteEndpointName, "LoconetOverTcp");
        }
        catch (Exception ex)
        {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError(ex, "TCP send failed to {RemoteEndpoint}", _stream.RemoteEndpointName);
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
        if (!_stream.IsConnected)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("TCP stream to {RemoteEndpoint} was not connected, connecting now", _stream.RemoteEndpointName);
            await _stream.ConnectAsync(cancellationToken).ConfigureAwait(false);
        }
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("TCP receive task starting for {RemoteEndpoint}", _stream.RemoteEndpointName);
        _receiveTask = ReceiveAsync(cancellationToken);
    }

    private async Task ReceiveAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var line = await _stream.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                if (line is null)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.LogInformation("TCP connection closed by {RemoteEndpoint}", _stream.RemoteEndpointName);
                    break;
                }
                ProcessLine(line);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation, exit silently
        }
        catch (ObjectDisposedException)
        {
            // Stream was disposed, exit silently
        }
        catch (Exception ex)
        {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError(ex, "TCP receive failed from {RemoteEndpoint}", _stream.RemoteEndpointName);
            _observers.Error(ex);
        }
    }

    /// <summary>
    /// Processes a single line from the LoconetOverTcp protocol.
    /// </summary>
    /// <param name="line">The protocol line to process.</param>
    internal void ProcessLine(string line)
    {
        if (string.IsNullOrEmpty(line)) return;

        var spaceIndex = line.IndexOf(' ');
        var token = spaceIndex >= 0 ? line[..spaceIndex] : line;
        var payload = spaceIndex >= 0 ? line[(spaceIndex + 1)..] : string.Empty;

        switch (token.ToUpperInvariant())
        {
            case "RECEIVE":
                var data = ParseHex(payload);
                if (data is not null && data.Length > 0)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug("TCP receive: {Data}", BitConverter.ToString(data));
                    _observers.Notify(CommunicationResult.Success(data, _stream.RemoteEndpointName, "LoconetOverTcp"));
                }
                else
                {
                    if (_logger.IsEnabled(LogLevel.Warning))
                        _logger.LogWarning("Invalid hex in RECEIVE line: {Line}", line);
                }
                break;

            case "VERSION":
                ServerVersion = payload;
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("Server version: {Version}", payload);
                break;

            case "SENT":
                if (payload.Equals("OK", StringComparison.OrdinalIgnoreCase))
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug("Server confirmed SENT OK");
                }
                else
                {
                    if (_logger.IsEnabled(LogLevel.Warning))
                        _logger.LogWarning("Server reported send error: SENT {Payload}", payload);
                }
                break;

            case "TIMESTAMP":
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("Server timestamp: {Payload}", payload);
                break;

            case "BREAK":
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning("Bus collision reported by server");
                break;

            case "ERROR":
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning("Server error: {Payload}", payload);
                break;

            default:
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning("Unknown LoconetOverTcp token: {Line}", line);
                break;
        }
    }

    /// <summary>
    /// Formats a byte array as uppercase, space-separated hex string.
    /// </summary>
    internal static string FormatHex(byte[] data) =>
        string.Join(' ', data.Select(b => b.ToString("X2", CultureInfo.InvariantCulture)));

    /// <summary>
    /// Parses a space-separated hex string into a byte array.
    /// </summary>
    /// <returns>The parsed bytes, or null if the input is invalid.</returns>
    internal static byte[]? ParseHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return null;
        try
        {
            var tokens = hex.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var result = new byte[tokens.Length];
            for (var i = 0; i < tokens.Length; i++)
            {
                result[i] = byte.Parse(tokens[i], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            return result;
        }
        catch (FormatException)
        {
            return null;
        }
        catch (OverflowException)
        {
            return null;
        }
    }

    private async Task CloseAsync()
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("TcpLocoNetChannel closing asynchronously for {RemoteEndpoint}", _stream.RemoteEndpointName);
        _stream.Close();
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
            _logger.LogInformation("TcpLocoNetChannel closing synchronously for {RemoteEndpoint}", _stream.RemoteEndpointName);
        _stream.Close();
        _observers.Completed();
    }

    #region IDisposable and IAsyncDisposable Support

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await CloseAsync().ConfigureAwait(false);
            _stream.Dispose();
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
                _stream.Dispose();
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
