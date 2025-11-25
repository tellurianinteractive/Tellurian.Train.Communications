using System.Net;
using System.Net.Sockets;

namespace Tellurian.Communications.Channels;

public sealed class UdpDataChannel(int localPort, IPEndPoint remoteEndPoint) : ICommunicationsChannel, IAsyncDisposable, IDisposable
{
    private readonly UdpClient _Client = new UdpClient(localPort);
    private readonly IPEndPoint _RemoteEndPoint = remoteEndPoint;
    private readonly Observers<CommunicationResult> _Observers = new Observers<CommunicationResult>();
    private Task? _ReceiveTask;

    public async Task<CommunicationResult> SendAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        if (data is null || data.Length == 0) return CommunicationResult.NoOperation();
        try
        {
            await _Client.SendAsync(data, _RemoteEndPoint, cancellationToken).ConfigureAwait(false);
            return CommunicationResult.Success(data, _RemoteEndPoint.ToString(), "UDP");
        }
        catch (Exception ex)
        {
            return CommunicationResult.Failure(ex);
        }
    }

    public IDisposable Subscribe(IObserver<CommunicationResult> observer)
    {
        return _Observers.Subscribe(observer);
    }

    public Task StartReceiveAsync(CancellationToken cancellationToken = default)
    {
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
                _Observers.Error(ex);
            }
        }
    }

    private async Task CloseAsync()
    {
        _Client.Close();
        if (_ReceiveTask is not null)
        {
            try
            {
                await _ReceiveTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Expected when caller cancels the token
            }
        }
        _Observers.Completed();
    }

    private void CloseSync()
    {
        _Client.Close();
        _Observers.Completed();
        // Note: We don't wait for _ReceiveTask to complete in synchronous dispose
        // to avoid potential deadlocks. Closing the client will cause ReceiveAsync to throw.
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
