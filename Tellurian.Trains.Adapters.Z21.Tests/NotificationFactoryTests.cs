namespace Tellurian.Trains.Adapters.Z21.Tests;

[TestClass]
public class NotificationFactoryTests
{
    private static Notification AssertCreatedCorrectNotification(Frame frame, Type expectedType)
    {
        var actual = frame.Notification();
        Assert.IsInstanceOfType(actual, expectedType);
        return actual;
    }

    [TestMethod]
    public void Notification_CreatesUnsupportedNotification_WhenHeaderUndefined()
    {
        var actual = new Frame(FrameHeader.Undefined).Notification();
        Assert.IsInstanceOfType<UnsupportedNotification>(actual);
    }

    [TestMethod]
    public void Notification_CreatesLocoAddressModeNotification()
    {
        var frame = new Frame(FrameHeader.LocoAddressMode, new byte[] { 0x00, 0x01, 0x01 });
        var actual = (LocoAddressModeNotification)AssertCreatedCorrectNotification(frame, typeof(LocoAddressModeNotification));
        Assert.AreEqual(1, actual.Address);
        Assert.AreEqual(AddressMode.MM, actual.Mode);
    }

    [TestMethod]
    public void Notification_CreatesHardwareInfoNotification()
    {
        var frame = new Frame(FrameHeader.HardwareInfo, new byte[] { 0x00, 0x02, 0x00, 0x00, 0x20, 0x01, 0x00, 0x00 });
        var actual = (HardwareInfoNotification)AssertCreatedCorrectNotification(frame, typeof(HardwareInfoNotification));
        Assert.AreEqual("Z21 old -2012", actual.Name);
        Assert.AreEqual("1.20", actual.Version);
    }

    [TestMethod]
    public void Notification_CreatesSerialNumberNotification()
    {
        const int serialNumber = 12345678;
        var frame = new Frame(FrameHeader.SerialNumber, BitConverter.GetBytes(serialNumber));
        var actual = (SerialNumberNotification)AssertCreatedCorrectNotification(frame, typeof(SerialNumberNotification));
        Assert.AreEqual(serialNumber, actual.SerialNumber);
    }

    [TestMethod]
    public void Notification_CreatesBroadcastSubjectsNotification()
    {
        const BroadcastSubjects subjects = BroadcastSubjects.All;
        var frame = new Frame(FrameHeader.SubscribeNotifications, BitConverter.GetBytes((int)subjects));
        var actual = (BroadcastSubjectsNotification)AssertCreatedCorrectNotification(frame, typeof(BroadcastSubjectsNotification));
        Assert.AreEqual(subjects, actual.Subjects);
    }
}
