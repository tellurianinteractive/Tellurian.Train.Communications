using Tellurian.Trains.Protocols.LocoNet;
using Tellurian.Trains.Protocols.LocoNet.Notifications;

namespace ins.Tellurian.Protocols.LocoNet.Tests;

[TestClass]
public class LongAcknowledgeTests
{
    [TestMethod]
    public void FifoIsFull()
    {
        byte[] data = new byte[] { 0xB4, 0x3D, 0x00 };
        var target = new LongAcknowledge(Message.AppendChecksum(data));
        Assert.IsFalse(target.IsSuccess);
        Assert.IsTrue(target.IsFailure);
        Assert.IsFalse(target.IsUndecided);
    }

    [TestMethod]
    public void Accepted()
    {
        byte[] data = new byte[] { 0xB4, 0x3D, 0x7F };
        var target = new LongAcknowledge(Message.AppendChecksum(data));
        Assert.IsTrue(target.IsSuccess);
        Assert.IsFalse(target.IsFailure);
        Assert.IsFalse(target.IsUndecided);
    }

    [TestMethod]
    public void IsUndecided()
    {
        byte[] data = new byte[] { 0xB4, 0x3B, 0x00 };
        var target = new LongAcknowledge(Message.AppendChecksum(data));
        Assert.IsFalse(target.IsSuccess);
        Assert.IsFalse(target.IsFailure);
        Assert.IsTrue(target.IsUndecided);
    }
}
