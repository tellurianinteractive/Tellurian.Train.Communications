using Tellurian.Trains.Protocols.XpressNet.Commands;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class SetPowerUpModeCommandTests
{
    [TestMethod]
    public void SetPowerUpMode_ReturnsCorrectBytes_ForManualMode()
    {
        var target = new SetPowerUpModeCommand(PowerUpMode.Manual);
        var data = target.GetData();

        Assert.AreEqual(0x22, data[0]);     // Header with length 2
        Assert.AreEqual(0x22, data[1]);     // Identification
        Assert.AreEqual(0x00, data[2]);     // Manual mode
    }

    [TestMethod]
    public void SetPowerUpMode_ReturnsCorrectBytes_ForAutomaticMode()
    {
        var target = new SetPowerUpModeCommand(PowerUpMode.Automatic);
        var data = target.GetData();

        Assert.AreEqual(0x22, data[0]);     // Header with length 2
        Assert.AreEqual(0x22, data[1]);     // Identification
        Assert.AreEqual(0x01, data[2]);     // Automatic mode
    }

    [TestMethod]
    public void SetPowerUpMode_StaticManual_ReturnsCorrectBytes()
    {
        var target = SetPowerUpModeCommand.Manual;
        var data = target.GetData();

        Assert.AreEqual(0x00, data[2]);     // Manual mode
    }

    [TestMethod]
    public void SetPowerUpMode_StaticAutomatic_ReturnsCorrectBytes()
    {
        var target = SetPowerUpModeCommand.Automatic;
        var data = target.GetData();

        Assert.AreEqual(0x01, data[2]);     // Automatic mode
    }
}
