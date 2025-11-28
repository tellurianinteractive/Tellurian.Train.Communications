using Tellurian.Trains.Protocols.XpressNet.Decoder;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class ServiceModeResultsCommandTests
{
    [TestMethod]
    public void ServiceModeResults_ReturnsCorrectBytes()
    {
        var target = new ServiceModeResultsCommand();
        var data = target.GetData();
        Assert.AreEqual(0x21, data[0]); // Header with length 1
        Assert.AreEqual(0x10, data[1]); // Identification
    }
}
