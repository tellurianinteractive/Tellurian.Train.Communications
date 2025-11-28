using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Tellurian.Trains.Communications.Channels;

public sealed class UdpDataChannel : ICommunicationsChannel, IAsyncDisposable, IDisposable
{
    private readonly UdpClient _Client;
    private readonly IPEndPoint _RemoteEndPoint;
    private readonly Observers<CommunicationResult> _Observers = new();
    private readonly ILogger _Logger;
    private Task? _ReceiveTask;

    public UdpDataChannel(int localPort, IPEndPoint remoteEndPoint, ILogger<UdpDataChannel> logger)
    {
        _Client = new UdpClient(localPort);
        _RemoteEndPoint = remoteEndPoint ?? throw new ArgumentNullException(nameof(remoteEndPoint));
        _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (_Logger.IsEnabled(LogLevel.Information))
            _Logger.LogInformation("UdpDataChannel created on local port {LocalPort} targeting {RemoteEndPoint}", localPort, remoteEndPoint);
    }

    public async Task<CommunicationResult> SendAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        if (data is null || data.Length == 0) return CommunicationResult.NoOperation();
        try
        {
            if (_Logger.IsEnabled(LogLevel.Debug))
                _Logger.LogDebug("UDP send: {Data}", BitConverter.ToString(data));
            await _Client.SendAsync(data, _RemoteEndPoint, cancellationToken).ConfigureAwait(false);
            return CommunicationResult.Success(data, _RemoteEndPoint.ToString(), "UDP");
        }
        catch (Exception ex)
        {
            if (_Logger.IsEnabled(LogLevel.Error))
                _Logger.LogError(ex, "UDP send failed");
            return CommunicationResult.Failure(ex);
        }
    }

    public IDisposable Subscribe(IObserver<CommunicationResult> observer)
    {
        return _Observers.Subscribe(observer);
    }

    public Task StartReceiveAsync(CancellationToken cancellationToken = default)
    {
        if (_Logger.IsEnabled(LogLevel.Information))
            _Logger.LogInformation("UDP receive task starting");
        _ReceiveTask = ReceiveAsync(cancellationToken);
        return Task.CompletedTask;
    }

    private async Task ReceiveAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await _Client.ReceiveAsync(cancellationToken).ConfigureAwait(false);
                if (_Logger.IsEnabled(LogLevel.Debug))
                    _Logger.LogDebug("UDP receive from {RemoteEndPoint}: {Data}", result.RemoteEndPoint, BitConverter.ToString(result.Buffer));
                _Observers.Notify(CommunicationResult.Success(result.Buffer, result.RemoteEndPoint.ToString(), "UDP"));
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        catch (SocketException ex)
        {
            if (ex.ErrorCode != 10004)
            {
                if (_Logger.IsEnabled(LogLevel.Error))
                    _Logger.LogError(ex, "UDP receive failed with socket error {ErrorCode}", ex.ErrorCode);
                _Observers.Error(ex);
            }
        }
    }

    private async Task CloseAsync()
    {
        if (_Logger.IsEnabled(LogLevel.Information))
            _Logger.LogInformation("UdpDataChannel closing asynchronously");
        _Client.Close();
        if (_ReceiveTask is not null)
        {
            try
            {
                await _ReceiveTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
        }
        _Observers.Completed();
    }

    private void CloseSync()
    {
        if (_Logger.IsEnabled(LogLevel.Information))
            _Logger.LogInformation("UdpDataChannel closing synchronously");
        _Client.Close();
        _Observers.Completed();
    }

    #region IDisposable and IAsyncDisposable Support
    private bool _disposedValue = false;

    public async ValueTask DisposeAsync()
    {
        if (!_disposedValue)
        {
            await CloseAsync().ConfigureAwait(false);
            _disposedValue = true;
        }
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                CloseSync();
            }
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
