using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using Tellurian.Trains.Communications.Channels;
using Tellurian.Trains.Communications.Interfaces.Decoder;
using Tellurian.Trains.Protocols.XpressNet.Decoder;


namespace Tellurian.Trains.Adapters.Z21.Tests;

/// <summary>
/// Dessa test kr�ver att Z21 �r ansluten till n�tverket.
/// </summary>
[Ignore("Z21 must be connected for these tests.")]
[TestCategory("Integration tests")]
[TestClass]
public class IntegrationTests
{
    private Adapter? Target;
    private ICommunicationsChannel? Channel;
    private readonly NotificationObserver Observer = new();
    private IDisposable? ObserverSubscription;

    public required TestContext TestContext { get; set; }

    [TestInitialize]
    public async Task TestInitialize()
    {
        Channel = new UdpDataChannel(31105, new IPEndPoint(IPAddress.Parse("192.168.0.111"), 21105), NullLogger<UdpDataChannel>.Instance);
        await StartAdapterAsync();
        await Task.Delay(100, TestContext.CancellationToken);
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        await StopAdapterAsync();
        if (Channel is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else
        {
            (Channel as IDisposable)?.Dispose();
        }
    }

    [TestMethod]
    public void AdapterStartsAndStops()
    {
        Assert.IsTrue(BitConverter.IsLittleEndian);
    }

    [TestMethod]
    public async Task SendingWorks()
    {
        await (Target?.GetSerialNumberAsync(TestContext.CancellationToken) ?? Task.FromResult(false));
        await Task.Delay(100, TestContext.CancellationToken);
        if (Observer.Notifications.Count == 0) Assert.Inconclusive("Is Z21 connected?");
    }

    [TestMethod]
    public async Task SetDrive()
    {
        var address = Tellurian.Trains.Communications.Interfaces.Locos.Address.From(999);
        var drive = new Tellurian.Trains.Communications.Interfaces.Locos.Drive { Direction = Tellurian.Trains.Communications.Interfaces.Locos.Direction.Backward, Speed = Tellurian.Trains.Communications.Interfaces.Locos.Speed.Set(Tellurian.Trains.Communications.Interfaces.Locos.LocoSpeedSteps.Steps126, 2) };
        await (Target?.DriveAsync(address, drive, TestContext.CancellationToken) ?? Task.FromResult(false));
        await Task.Delay(100, TestContext.CancellationToken);
    }
    [TestMethod]
    public async Task GetHardwareVersion()
    {
        await (Target?.SendAsync(new GetHardwareInfoCommand(), TestContext.CancellationToken) ?? Task.FromResult(false));
        await Task.Delay(1000, TestContext.CancellationToken);
        Assert.IsNotEmpty(Observer.Notifications);
        var response = Observer.Notifications[0];
        Assert.AreEqual("Hardware Z21 old -2012 1.43", response.ToString());
    }

    [TestMethod]
    [Ignore]
    public async Task ReadLocoCV()
    {
        await (Target?.SendAsync(new ReadCVCommand(29), TestContext.CancellationToken) ?? Task.FromResult(false));
        await Task.Delay(3000, TestContext.CancellationToken);
        Assert.IsNotEmpty(Observer.Notifications);
        var response = Observer.Notifications[0];
        Assert.IsInstanceOfType<DecoderResponse>(response);
    }

    private async Task StartAdapterAsync()
    {
        Target = new Adapter(Channel, NullLogger<Adapter>.Instance);
        ObserverSubscription = Target.Subscribe(Observer);
        await Target.StartReceiveAsync(TestContext.CancellationToken);
    }

    private async Task StopAdapterAsync()
    {
        await (Target?.SendAsync(new LogOffCommand(), TestContext.CancellationToken) ?? Task.FromResult(false));
        ObserverSubscription?.Dispose();
        Target?.Dispose();
    }
}

internal class NotificationObserver : IObserver<Tellurian.Trains.Communications.Interfaces.Notification>
{
    public IList<Tellurian.Trains.Communications.Interfaces.Notification> Notifications = [];
    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
        throw error;
    }

    public void OnNext(Tellurian.Trains.Communications.Interfaces.Notification value)
    {
        Notifications.Add(value);
    }
}
