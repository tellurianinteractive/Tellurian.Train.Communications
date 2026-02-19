namespace Tellurian.Trains.Adapters.Z21.Tests;

[TestClass]
public class CommandTests
{
    private static Frame Act(Command target)
    {
        return target.ToFrame();
    }

    private static void AssertCorrectFrameData(int expectedLength, byte[] expectedData, Frame actual)
    {
        var expectedDataLength = expectedLength - 4;
        Assert.HasCount(expectedDataLength, expectedData, "Inconsistent expected lenght/data.");
        Assert.AreEqual(expectedLength, actual.Length, "Incorrect length of frame.");
        Assert.HasCount(expectedDataLength, actual.Data, "Incorrect length of data.");
        for (int i = 0; i < expectedDataLength; i++)
        {
            Assert.AreEqual(expectedData[i], actual.Data[i], "Invalid data at position " + i);
        }
    }

    [TestMethod]
    public void CreatesEmergencyStopCommand()
    {
        AssertCorrectFrameData(8, BitConverter.GetBytes((int)BroadcastSubjects.All), Act(new SubscribeNotificationsCommand(BroadcastSubjects.All)));
    }

    [TestMethod]
    public void CreatesGetHardwareInfoCommand()
    {
        AssertCorrectFrameData(4, Array.Empty<byte>(), Act(new GetHardwareInfoCommand()));
    }

    [TestMethod]
    public void CreatesGetLocomotiveAddressMode()
    {
        const short address = 12345;
        AssertCorrectFrameData(6, new byte[] { 0x30, 0x39 }, Act(new GetLocoAddressMode(address)));
    }

    [TestMethod]
    public void CreatesGetSubscribedBroadcastCommand()
    {
        AssertCorrectFrameData(4, Array.Empty<byte>(), Act(new GetSubscribedNotificationsCommand()));
    }

    [TestMethod]
    public void CreatesGetSystemStateCommand()
    {
        AssertCorrectFrameData(4, Array.Empty<byte>(), Act(new GetSystemStateCommand()));
    }

    [TestMethod]
    public void CreatesLogOffCommand()
    {
        AssertCorrectFrameData(4, Array.Empty<byte>(), Act(new LogOffCommand()));
    }

    [TestMethod]
    public void CreatesGetSerialNumberCommand()
    {
        AssertCorrectFrameData(4, Array.Empty<byte>(), Act(new GetSerialNumberCommand()));
    }
    [TestMethod]
    public void CreatesGetTurnoutAddressModeCommand()
    {
        const short address = 12345;
        AssertCorrectFrameData(6, new byte[] { 0x30, 0x39 }, Act(new GetTurnoutAddressMode(address)));
    }

    [TestMethod]
    public void CreatesSetLocomotiveAddressModeCommand()
    {
        const short address = 1;
        AssertCorrectFrameData(7, new byte[] { 0x00, 0x01, 0x01 }, Act(new SetLocoAddressModeCommand(address, AddressMode.MM)));
    }

    [TestMethod]
    public void CreatesLocoNetDetectorRequestCommand()
    {
        var command = new LocoNetDetectorRequestCommand(DetectorRequestType.StationaryInterrogate, 5);
        var frame = Act(command);
        Assert.AreEqual(FrameHeader.LocoNetDetector, frame.Header);
        AssertCorrectFrameData(7, new byte[] { 0x80, 0x05, 0x00 }, frame);
    }

    [TestMethod]
    public void CreatesCanDetectorRequestCommand()
    {
        var command = new CanDetectorRequestCommand(0xD000);
        var frame = Act(command);
        Assert.AreEqual(FrameHeader.CanDetector, frame.Header);
        AssertCorrectFrameData(7, new byte[] { 0x00, 0x00, 0xD0 }, frame);
    }

    [TestMethod]
    public void CreatesSetTurnoutAddressModeCommand()
    {
        const short address = 1;
        AssertCorrectFrameData(7, new byte[] { 0x00, 0x01, 0x01 }, Act(new SetTurnoutAddressModeCommand(address, AddressMode.MM)));
    }
}
