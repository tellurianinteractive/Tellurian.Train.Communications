using System.Net;
using System.Net.Sockets;

namespace Tellurian.Communications.Channels;

public sealed class UdpDataChannel : ICommunicationsChannel, IDisposable
{
    private readonly UdpClient _Client;
    private readonly IPEndPoint _RemoteEndPoint;
    private readonly Observers<CommunicationResult> _Observers = new Observers<CommunicationResult>();
    private readonly Task _ReceiveTask;

    public UdpDataChannel(int localPort, IPEndPoint remoteEndPoint)
    {
        _Client = new UdpClient(localPort);
        _RemoteEndPoint = remoteEndPoint;
        _ReceiveTask = new Task(() => Receive());
    }

    public CommunicationResult Send(byte[] data)
    {
        if (data is null || data.Length == 0) return CommunicationResult.NoOperation();
        try
        {
            var sentBytes = _Client.Send(data, data.Length, _RemoteEndPoint);
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

    public void StartReceive()
    {
        _ReceiveTask.Start();
    }

    private void Receive()
    {
        var remoteEndpoint = _RemoteEndPoint;
        try
        {
            while (true)
            {
                var data = _Client.Receive(ref remoteEndpoint);
                _Observers.Notify(CommunicationResult.Success(data, remoteEndpoint.ToString(), "UDP"));
            }
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

    private void Close()
    {
        _Client.Close();
        _ReceiveTask.Wait();
        _ReceiveTask.Dispose();
        _Observers.Completed();
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Close();
            }
            disposedValue = true;
        }
    }
    public void Dispose()
    {
        Dispose(true);
    }
    #endregion
}
