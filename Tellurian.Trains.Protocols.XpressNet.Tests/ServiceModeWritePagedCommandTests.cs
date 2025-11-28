using Tellurian.Trains.Protocols.XpressNet.Decoder;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class ServiceModeWritePagedCommandTests
{
    [TestMethod]
    public void PagedModeWrite_ReturnsCorrectBytes_ForCV1()
    {
        var target = new ServiceModeWritePagedCommand(1, 0x07);
        var data = target.GetData();
        Assert.AreEqual(0x23, data[0]); // Header with length 3
        Assert.AreEqual(0x17, data[1]); // Identification for Paged Mode
        Assert.AreEqual(0x01, data[2]); // CV 1
        Assert.AreEqual(0x07, data[3]); // Value
    }

    [TestMethod]
    public void PagedModeWrite_ReturnsCorrectBytes_ForCV256()
    {
        var target = new ServiceModeWritePagedCommand(256, 0x42);
        var data = target.GetData();
        Assert.AreEqual(0x23, data[0]);
        Assert.AreEqual(0x17, data[1]);
        Assert.AreEqual(0x00, data[2]); // CV 256 is encoded as 0x00
        Assert.AreEqual(0x42, data[3]);
    }
}
