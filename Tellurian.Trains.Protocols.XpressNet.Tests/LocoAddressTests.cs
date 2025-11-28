using Tellurian.Trains.Interfaces.Locos;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class LocoAddressTests {

    [TestMethod]
    public void Constructor_AllowsZero_AsSentinelValue() {
        var address = LocoAddress.Zero;
        Assert.AreEqual(0, address.Number);
    }

    [TestMethod]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenNegative() {
        Assert.Throws<ArgumentOutOfRangeException>(() => LocoAddress.From(-1));
    }

    [TestMethod]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenAbove9999() {
        Assert.Throws<ArgumentOutOfRangeException>(() => LocoAddress.From(10000));
    }

    [TestMethod]
    public void ShortAddress_HasCorrectProperties() {
        var target = LocoAddress.From(1);
        Assert.IsTrue(target.IsShort);
        Assert.IsFalse(target.IsLong);
        Assert.IsFalse(target.IsShortThreeDigit);
    }

    [TestMethod]
    public void LongAddress_HasCorrectProperties() {
        var target = LocoAddress.From(128);
        Assert.IsFalse(target.IsShort);
        Assert.IsTrue(target.IsLong);
        Assert.IsFalse(target.IsShortThreeDigit);
    }

    [TestMethod]
    public void GetBytesAccordingToXpressNet_ReturnsCorrectBytes_WhenShortAddress() {
        var target = LocoAddress.From(127);
        var actual = target.GetBytesAccordingToXpressNet();
        Assert.IsNotNull(actual);
        Assert.HasCount(2, actual);
        Assert.AreEqual(127, ((actual[0] & 0x3F) << 8) + actual[1]);
        Assert.AreEqual(0x00, (actual[0] & 0xC0));
        Assert.IsTrue(target.IsShortThreeDigit);
    }

    [TestMethod]
    public void GetBytesAccordingToXpressNet_ReturnsCorrectBytes_WhenLongAddress() {
        var target = LocoAddress.From(128);
        var actual = target.GetBytesAccordingToXpressNet();
        Assert.IsNotNull(actual);
        Assert.HasCount(2, actual);
        Assert.AreEqual(128, ((actual[0] & 0x3F) << 8) + actual[1]);
        Assert.AreEqual(0xC0, (actual[0] & 0xC0));
    }

    [TestMethod]
    public void Constructor_FromBytes_ReconstructsAddress() {
        var target = LocoAddress.From(241);
        var actual = LocoAddressExtensions.FromXpressNet(target.GetBytesAccordingToXpressNet());
        Assert.AreEqual(241, actual.Number);
     }
}
