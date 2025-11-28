using Tellurian.Trains.Protocols.XpressNet.Commands;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class AddressInquiryStackCommandTests
{
    [TestMethod]
    public void AddressInquiryStack_ReturnsCorrectBytes_ForwardSearch()
    {
        var target = new AddressInquiryStackCommand(new LocoAddress(1234));
        var data = target.GetData();

        Assert.AreEqual(0xE3, data[0]);
        Assert.AreEqual(0x05, data[1]);
        Assert.AreEqual(0xC4, data[2]);
        Assert.AreEqual(0xD2, data[3]);
    }

    [TestMethod]
    public void AddressInquiryStack_ReturnsCorrectBytes_BackwardSearch()
    {
        var target = new AddressInquiryStackCommand(new LocoAddress(3), SearchDirection.Backward);
        var data = target.GetData();

        Assert.AreEqual(0x06, data[1]);
    }

    [TestMethod]
    public void AddressInquiryStack_ReturnsCorrectBytes_GetFirst()
    {
        var target = new AddressInquiryStackCommand();
        var data = target.GetData();

        Assert.AreEqual(0x00, data[2]);
        Assert.AreEqual(0x00, data[3]);
    }
}
