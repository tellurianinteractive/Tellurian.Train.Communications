using Tellurian.Trains.Protocols.LocoNet.Notifications;

namespace Tellurian.Trains.Protocols.LocoNet.Tests;

[TestClass]
public class LongAcknowledgeTests
{
    [TestMethod]
    public void LongAcknowledge_IsFailure_WhenFifoIsFull()
    {
        byte[] data = [0xB4, 0x3D, 0x00];
        var target = new LongAcknowledge(Message.AppendChecksum(data));
        Assert.IsFalse(target.IsSuccess);
        Assert.IsTrue(target.IsFailure);
        Assert.IsFalse(target.IsUndecided);
    }

    [TestMethod]
    public void LongAcknowledge_IsSuccess_WhenAccepted()
    {
        byte[] data = [0xB4, 0x3D, 0x7F];
        var target = new LongAcknowledge(Message.AppendChecksum(data));
        Assert.IsTrue(target.IsSuccess);
        Assert.IsFalse(target.IsFailure);
        Assert.IsFalse(target.IsUndecided);
    }

    [TestMethod]
    public void LongAcknowledge_IsUndecided_WhenUnknownCode()
    {
        byte[] data = [0xB4, 0x3B, 0x00];
        var target = new LongAcknowledge(Message.AppendChecksum(data));
        Assert.IsFalse(target.IsSuccess);
        Assert.IsFalse(target.IsFailure);
        Assert.IsTrue(target.IsUndecided);
    }
}
