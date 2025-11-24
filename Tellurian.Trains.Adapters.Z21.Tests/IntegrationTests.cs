using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using Tellurian.Communications.Channels;
using Tellurian.Trains.Protocols.XpressNet.Decoder;
using Tellurian.Trains.Interfaces.Extensions;
using Tellurian.Trains.Interfaces.Decoder;


#pragma warning disable CA1001 // Dispose is handled in TestCleanup()

namespace Tellurian.Trains.Adapters.Z21.Tests;

/// <summary>
/// Dessa test kräver att Z21 är ansluten till nätverket.
/// </summary>
[Ignore("Z21 must be connected for these tests.")]
[TestCategory("Integration tests")]
[TestClass]
public class IntegrationTests
{
    private Adapter? Target;
    private ICommunicationsChannel? Channel;
    private readonly NotificationObserver Observer = new NotificationObserver();
    private IDisposable? ObserverSubscription;

    [TestInitialize]
    public void TestInitialize()
    {
        Channel = new UdpDataChannel(31105, new IPEndPoint(IPAddress.Parse("192.168.0.111"), 21105));
        StartAdapter();
        Thread.Sleep(100);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        StopAdapter();
        (Channel as IDisposable)?.Dispose();
    }

    [TestMethod]
    public void AdapterStartsAndStops()
    {
        Assert.IsTrue(BitConverter.IsLittleEndian);
    }

    [TestMethod]
    public void SendingWorks()
    {
        Target?.GetSerialNumber();
        Thread.Sleep(100);
        if (Observer.Notifications.Count == 0) Assert.Inconclusive("Is Z21 connected?");
    }

    [TestMethod]
    public void SetDrive()
    {
        var address = Interfaces.Locos.LocoAddress.From(999);
        var drive = new Interfaces.Locos.LocoDrive { Direction = Interfaces.Locos.LocoDirection.Backward, Speed = Interfaces.Locos.LocoSpeed.Set(Interfaces.Locos.LocoSpeedSteps.Steps126, 2)};
        Target?.Drive(address, drive);
        Thread.Sleep(100);
    }
    [TestMethod]
    public void GetHardwareVersion()
    {
        Target?.Send(new GetHardwareInfoCommand());
        Thread.Sleep(1000);
        Assert.IsNotEmpty(Observer.Notifications);
        var response = Observer.Notifications[0];
        Assert.AreEqual("Hardware Z21 old -2012 1.41", response.ToString());
    }

    [TestMethod]
    [Ignore]
    public void ReadLocoCV()
    {
        Target?.Send(new ReadCVCommand(29.CV()));
        Thread.Sleep(3000);
        Assert.IsNotEmpty(Observer.Notifications);
        var response = Observer.Notifications[0];
        Assert.IsInstanceOfType(response, typeof(DecoderResponse));
    }

    private void StartAdapter()
    {
        Target = new Adapter(Channel, NullLogger<Adapter>.Instance);
        ObserverSubscription = Target.Subscribe(Observer);
        Target.StartReceive();
    }

    private void StopAdapter()
    {
        Target?.Send(new LogOffCommand());
        ObserverSubscription?.Dispose();
        Target?.Dispose();
    }
}

internal class NotificationObserver : IObserver<Interfaces.Notification>
{
    public IList<Interfaces.Notification> Notifications = new List<Interfaces.Notification>();
    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
        throw error;
    }

    public void OnNext(Interfaces.Notification value)
    {
        Notifications.Add(value);
    }
}
