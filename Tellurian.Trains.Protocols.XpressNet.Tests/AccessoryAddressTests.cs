namespace Tellurian.Trains.Protocols.XpressNet.Tests; 
[TestClass]
public class AccessoryAddressTests {

    [TestMethod]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenZero() {
        Assert.Throws<ArgumentOutOfRangeException>(() => new AccessoryAddress(0));
    }

    [TestMethod]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenNegative() {
        Assert.Throws<ArgumentOutOfRangeException>(() => new AccessoryAddress(-1));
    }

    [TestMethod]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenAbove9999() {
        Assert.Throws<ArgumentOutOfRangeException>(() => new AccessoryAddress(10000));
    }

    [TestMethod]
    public void AccessoryAddress_HasCorrectProperties() {
        var target = new AccessoryAddress(777);
        Assert.AreEqual(194, target.Group);
        Assert.AreEqual(1, target.Subaddress);
     }

    [TestMethod]
    public void GetBytes_ReturnsCorrectBytes() {
        var target = new AccessoryAddress(777);
        var data = target.GetBytes();
        var actual = (data[0] << 8) + data[1] + 1;
        Assert.AreEqual(777, actual);
    }


}
