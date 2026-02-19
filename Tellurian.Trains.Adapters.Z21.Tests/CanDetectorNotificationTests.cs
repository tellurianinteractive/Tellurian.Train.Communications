namespace Tellurian.Trains.Adapters.Z21.Tests;

[TestClass]
public class CanDetectorNotificationTests
{
    private static CanDetectorNotification CreateNotification(ushort networkId, ushort moduleAddress, byte port, byte type, ushort value1, ushort value2)
    {
        var data = new byte[10];
        BitConverter.GetBytes(networkId).CopyTo(data, 0);
        BitConverter.GetBytes(moduleAddress).CopyTo(data, 2);
        data[4] = port;
        data[5] = type;
        BitConverter.GetBytes(value1).CopyTo(data, 6);
        BitConverter.GetBytes(value2).CopyTo(data, 8);
        return new CanDetectorNotification(new Frame(FrameHeader.CanDetector, data));
    }

    [TestMethod]
    public void ParsesOccupancyFree()
    {
        var n = CreateNotification(0xD000, 1, 3, 0x01, (ushort)CanOccupancyStatus.Free, 0);
        Assert.AreEqual((byte)0x01, n.DetectorType);
        Assert.AreEqual(CanOccupancyStatus.Free, n.OccupancyStatus);
        Assert.IsFalse(n.IsOccupied);
    }

    [TestMethod]
    public void ParsesOccupancyOccupied()
    {
        var n = CreateNotification(0xD000, 1, 3, 0x01, (ushort)CanOccupancyStatus.Occupied, 0);
        Assert.IsTrue(n.IsOccupied);
        Assert.AreEqual(CanOccupancyStatus.Occupied, n.OccupancyStatus);
    }

    [TestMethod]
    public void ParsesOccupancyOccupiedWithVoltage()
    {
        var n = CreateNotification(0xD000, 2, 5, 0x01, (ushort)CanOccupancyStatus.OccupiedWithVoltage, 0);
        Assert.IsTrue(n.IsOccupied);
        Assert.AreEqual(CanOccupancyStatus.OccupiedWithVoltage, n.OccupancyStatus);
    }

    [TestMethod]
    public void ParsesRailComAddresses()
    {
        // Type 0x11, Value1 = loco 42 with forward direction, Value2 = loco 100 no direction
        ushort value1 = (ushort)(42 | 0xC000); // Address 42, direction valid + forward
        ushort value2 = 100; // Address 100, no direction info
        var n = CreateNotification(0xD001, 1, 0, 0x11, value1, value2);

        Assert.IsTrue(n.IsRailCom);
        Assert.AreEqual((ushort)42, n.LocoAddress1);
        Assert.IsTrue(n.Direction1IsValid);
        Assert.IsTrue(n.Direction1IsForward);
        Assert.AreEqual((ushort)100, n.LocoAddress2);
        Assert.IsFalse(n.Direction2IsValid);
    }

    [TestMethod]
    public void ParsesNetworkIdAndModuleAddress()
    {
        var n = CreateNotification(0xD042, 5, 7, 0x01, 0, 0);
        Assert.AreEqual((ushort)0xD042, n.NetworkId);
        Assert.AreEqual((ushort)5, n.ModuleAddress);
        Assert.AreEqual((byte)7, n.Port);
    }

    [TestMethod]
    public void ParsesHighRailComType()
    {
        var n = CreateNotification(0xD000, 1, 0, 0x1F, 0, 0);
        Assert.IsTrue(n.IsRailCom);
        Assert.AreEqual((byte)0x1F, n.DetectorType);
    }
}
