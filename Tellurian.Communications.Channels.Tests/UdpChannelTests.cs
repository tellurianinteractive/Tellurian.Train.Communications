using System.Net;
using System.Net.Sockets;

namespace Tellurian.Communications.Channels.Tests;

[TestClass]
public class UdpChannelTests
{
    private ICommunicationsChannel? Target;
    private byte[]? ReceivedData;
    private IPEndPoint? Destination;

    [TestInitialize]
    public void TestInitialize()
    {
        Destination = new IPEndPoint(IPAddress.Loopback, 9901);
        Target = new UdpDataChannel(9902, Destination);
        ReceivedData = null;
    }

    [TestCleanup]
    public void TestCleanup()
    {
        (Target as IDisposable)?.Dispose();
    }

    [TestMethod]
    public void SendsPacket()
    {
        var listen = new IPEndPoint(IPAddress.Any, 9901);
        Target?.StartReceive();
        using var receiver = new UdpClient(listen);
        var state = new UdpState { Client = receiver, Source = listen };
        receiver.BeginReceive(OnReceive, state);
        var data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
        Target?.Send(data);
        Target?.Send(data);
        Thread.Sleep(100);
        Assert.IsNotNull(ReceivedData);
        Assert.AreEqual(data.Length, ReceivedData?.Length);
    }

    private void OnReceive(IAsyncResult result)
    {
        if (!(result.AsyncState is UdpState state)) return;
        var client = state.Client;
        var source = state.Source;
        ReceivedData = client is null ? Array.Empty<byte>() : client.EndReceive(result, ref source);
    }

    [TestMethod]
    public void ReceivesPacket()
    {
        var listener = new DataObserver();
        var data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
        using (var subscribtion = Target?.Subscribe(listener))
        {
            Target?.StartReceive();
            using var sender = new UdpClient(9901);
            sender.Connect(IPAddress.Loopback, 9902);
            sender.Send(data, data.Length);
            sender.Send(data, data.Length);
            Thread.Sleep(100);
        }
        Assert.IsNotNull(listener.ReceivedData);
        Assert.HasCount(2, listener.ReceivedData);
        Assert.AreEqual(data.Length, listener.ReceivedData[0].Length);
    }
}

internal class DataObserver : IObserver<CommunicationResult>
{
    public readonly List<CommunicationResult> ReceivedData = new List<CommunicationResult>();

    void IObserver<CommunicationResult>.OnCompleted()
    {
    }

    void IObserver<CommunicationResult>.OnError(Exception error)
    {
        throw error;
    }

    void IObserver<CommunicationResult>.OnNext(CommunicationResult value)
    {
        ReceivedData.Add(value);
    }
}

