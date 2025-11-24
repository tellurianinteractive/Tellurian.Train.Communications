namespace Tellurian.Trains.Adapters.Z21.Tests;

[TestClass]
public class FrameTests
{
    [TestMethod]
    public void CreateWithoutDataGivesZeroLengthData()
    {
        var target = new Frame(FrameHeader.Test);
        Assert.AreEqual(FrameHeader.Test, target.Header);
        Assert.IsFalse(target.Data.Any());
    }

    [TestMethod]
    public void CreateWithNullDataThrows()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new Frame(FrameHeader.Test, null));
    }

    [TestMethod]
    public void TestFrameHasFourBytesData()
    {
        var target = new Frame(FrameHeader.Test);
        Assert.AreEqual(4, target.Length);
    }
}
