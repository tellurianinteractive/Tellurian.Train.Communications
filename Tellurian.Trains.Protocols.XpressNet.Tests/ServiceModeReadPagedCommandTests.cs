using Tellurian.Trains.Protocols.XpressNet.Decoder;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class ServiceModeReadPagedCommandTests
{
    [TestMethod]
    public void PagedModeRead_ReturnsCorrectBytes_ForCV1()
    {
        var target = new ServiceModeReadPagedCommand(1);
        var data = target.GetData();
        Assert.AreEqual(0x22, data[0]); // Header with length 2
        Assert.AreEqual(0x14, data[1]); // Identification for Paged Mode
        Assert.AreEqual(0x01, data[2]); // CV 1
    }

    [TestMethod]
    public void PagedModeRead_ReturnsCorrectBytes_ForCV256()
    {
        var target = new ServiceModeReadPagedCommand(256);
        var data = target.GetData();
        Assert.AreEqual(0x22, data[0]);
        Assert.AreEqual(0x14, data[1]);
        Assert.AreEqual(0x00, data[2]); // CV 256 is encoded as 0x00
    }
}
