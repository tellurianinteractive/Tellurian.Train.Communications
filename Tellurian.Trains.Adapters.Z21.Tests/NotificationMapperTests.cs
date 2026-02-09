namespace Tellurian.Trains.Adapters.Z21.Tests;

[TestClass]
public class NotificationMapperTests
{
    [TestMethod]
    public void MapNullNotificationReturnsEmptyArray()
    {
        var result = NotificationMapper.Map(null!);
        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void MapUnsupportedNotificationReturnsUnmapped()
    {
        var frame = new Frame(FrameHeader.Undefined);
        var unsupported = new UnsupportedNotification(frame);

        var result = unsupported.Map();

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        // Unsupported notifications should map to a message notification
        Assert.IsInstanceOfType<Communications.Interfaces.MessageNotification>(result[0]);
    }

    [TestMethod]
    public void MapBroadcastSubjectsNotificationReturnsUnmapped()
    {
        // BroadcastSubjectsNotification is not in the mappings dictionary
        // so it should return an unmapped message
        const BroadcastSubjects subjects = BroadcastSubjects.All;
        var frame = new Frame(FrameHeader.BroadcastSubjects, BitConverter.GetBytes((int)subjects));
        var notification = new BroadcastSubjectsNotification(frame);

        var result = notification.Map();

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.IsInstanceOfType<Communications.Interfaces.MessageNotification>(result[0]);
    }

    [TestMethod]
    public void MapLocoNetNotificationWithNullMessage()
    {
        var frame = new Frame(FrameHeader.LocoNetReceive, new byte[] { 0x00 });
        var notification = new LocoNetNotification(frame);

        var result = notification.Map();

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void MapSerialNumberNotificationReturnsUnmapped()
    {
        // SerialNumberNotification is not in the mappings dictionary
        const int serialNumber = 12345678;
        var frame = new Frame(FrameHeader.SerialNumber, BitConverter.GetBytes(serialNumber));
        var notification = new SerialNumberNotification(frame);

        var result = notification.Map();

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.IsInstanceOfType<Communications.Interfaces.MessageNotification>(result[0]);
    }

    [TestMethod]
    public void MapHardwareInfoNotificationReturnsUnmapped()
    {
        // HardwareInfoNotification is not in the mappings dictionary
        var frame = new Frame(FrameHeader.HardwareInfo, new byte[] { 0x00, 0x02, 0x00, 0x00, 0x20, 0x01, 0x00, 0x00 });
        var notification = new HardwareInfoNotification(frame);

        var result = notification.Map();

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.IsInstanceOfType<Communications.Interfaces.MessageNotification>(result[0]);
    }
}
