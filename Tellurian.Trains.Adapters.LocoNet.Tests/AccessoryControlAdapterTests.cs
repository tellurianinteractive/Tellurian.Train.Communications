using Microsoft.Extensions.Logging.Abstractions;
using Tellurian.Trains.Communications.Interfaces.Accessories;
using Tellurian.Trains.Protocols.LocoNet.Commands;

namespace Tellurian.Trains.Adapters.LocoNet.Tests;

[TestClass]
public class AccessoryControlAdapterTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    public async Task SetAccessoryAsync_SendsSetAccessoryCommand()
    {
        var channel = new MockChannel();
        var adapter = new Adapter(channel, NullLogger<Adapter>.Instance);

        var result = await adapter.SetAccessoryAsync(
            Address.From(100),
            AccessoryCommand.Throw(activate: true),
            TestContext.CancellationToken);

        Assert.IsTrue(result);
        Assert.HasCount(1, channel.SentData);

        // Verify it's a SetAccessory command (0xB0)
        Assert.AreEqual(SetAccessoryCommand.OperationCode, channel.SentData[0][0]);
    }

    [TestMethod]
    public async Task SetThrownAsync_SendsSetAccessoryCommand()
    {
        var channel = new MockChannel();
        var adapter = new Adapter(channel, NullLogger<Adapter>.Instance);

        var result = await adapter.SetThrownAsync(Address.From(50), true, TestContext.CancellationToken);

        Assert.IsTrue(result);
        Assert.HasCount(1, channel.SentData);
        Assert.AreEqual(SetAccessoryCommand.OperationCode, channel.SentData[0][0]);
    }

    [TestMethod]
    public async Task SetClosedAsync_SendsSetAccessoryCommand()
    {
        var channel = new MockChannel();
        var adapter = new Adapter(channel, NullLogger<Adapter>.Instance);

        var result = await adapter.SetClosedAsync(Address.From(75), true, TestContext.CancellationToken);

        Assert.IsTrue(result);
        Assert.HasCount(1, channel.SentData);
        Assert.AreEqual(SetAccessoryCommand.OperationCode, channel.SentData[0][0]);
    }

    [TestMethod]
    public async Task TurnOffAsync_SendsSetAccessoryCommand()
    {
        var channel = new MockChannel();
        var adapter = new Adapter(channel, NullLogger<Adapter>.Instance);

        var result = await adapter.TurnOffAsync(Address.From(25), TestContext.CancellationToken);

        Assert.IsTrue(result);
        Assert.HasCount(1, channel.SentData);
        Assert.AreEqual(SetAccessoryCommand.OperationCode, channel.SentData[0][0]);
    }

    [TestMethod]
    public async Task QueryAccessoryStateAsync_SendsRequestAccessoryStateCommand()
    {
        var channel = new MockChannel();
        var adapter = new Adapter(channel, NullLogger<Adapter>.Instance);

        var result = await adapter.QueryAccessoryStateAsync(Address.From(10), TestContext.CancellationToken);

        Assert.IsTrue(result);
        Assert.HasCount(1, channel.SentData);

        // Verify it's a RequestAccessoryState command (0xBC)
        Assert.AreEqual(RequestAccessoryStateCommand.OperationCode, channel.SentData[0][0]);
    }

    [TestMethod]
    public async Task SetAccessoryAsync_ReturnsFailureWhenChannelFails()
    {
        var channel = new MockChannel { ShouldFail = true };
        var adapter = new Adapter(channel, NullLogger<Adapter>.Instance);

        var result = await adapter.SetAccessoryAsync(
            Address.From(100),
            AccessoryCommand.Throw(activate: true),
            TestContext.CancellationToken);

        Assert.IsFalse(result);
    }
}
