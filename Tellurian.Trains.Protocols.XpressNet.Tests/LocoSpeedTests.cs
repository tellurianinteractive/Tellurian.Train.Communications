namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class LocoSpeedTests
{
    [TestMethod]
    public void PropertiesAreSet()
    {
        var target = LocoSpeed.FromCode(0, 5);
        Assert.AreEqual(5, target.Current);
        Assert.AreEqual(15, target.GetSpeed(1.0f));
        Assert.AreEqual(0, target.GetSpeed(0.0f));
        Assert.AreEqual(4, target.Step(3));
    }
}
