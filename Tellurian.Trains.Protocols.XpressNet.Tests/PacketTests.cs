using Tellurian.Trains.Protocols.XpressNet.Commands;
using Tellurian.Trains.Protocols.XpressNet.Decoder;
using Tellurian.Trains.Protocols.XpressNet.Notifications;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class PacketTests
{
    [TestMethod]
    public void GetBytes_ReturnsCorrectBytes()
    {
        var target = new Packet(new TestMessage());
        var actual = target.GetBytes();
        Assert.HasCount(3, actual);
        Assert.AreEqual(0x61, actual[0]);
        Assert.AreEqual(0x01, actual[1]);
        Assert.AreEqual(0x60, actual[2]);
    }

    [TestMethod]
    public void Notification_CreatesCorrectTypes_ForAllBroadcasts()
    {
        AssertCreatedMessage<TrackPowerOffBroadcast>(new byte[] { 0x61, 0x00, 0x61 });
        AssertCreatedMessage<TrackPowerOnBroadcast>(new byte[] { 0x61, 0x01, 0x60 });
        AssertCreatedMessage<ProgrammingModeEnteredBroadcast>(new byte[] { 0x61, 0x02, 0x63 });
        AssertCreatedMessage<WriteCVShortCircuitResponse>(new byte[] { 0x61, 0x12, 0x73 });
        AssertCreatedMessage<WriteCVTimeoutResponse>(new byte[] { 0x61, 0x13, 0x72 });
        AssertCreatedMessage<ProgrammingStationBusyBroadcast>(new byte[] { 0x61, 0x1F, 0x7E });
        AssertCreatedMessage<ProgrammingStationReadyBroadcast>(new byte[] { 0x61, 0x11, 0x70 });
        AssertCreatedMessage<ProgrammingModeEnteredBroadcast>(new byte[] { 0x61, 0x02, 0x63 });
        AssertCreatedMessage<EmergencyStopBroadcast>(new byte[] { 0x81, 0x00, 0x81 });
    }

    private static void AssertCreatedMessage<T>(byte[] data)
    {
        var target = new Packet(data);
        var actual = target.Notification;
        Assert.IsInstanceOfType<T>(actual);
    }

    private class TestMessage : Command
    {
        public TestMessage() : base(0x60, 0x01) { }
    }
}
