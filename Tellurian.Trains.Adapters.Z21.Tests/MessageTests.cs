namespace Tellurian.Trains.Adapters.Z21.Tests;

[TestClass]
public class MessageTests
{
    [TestMethod]
    public void Constructor_SetsTimestamp()
    {
        var target = new TestMessage();
        Assert.IsTrue(DateTimeOffset.Now.AddMilliseconds(-10) < target.Timestamp);
    }

    private class TestMessage : Message
    {
    }
}
