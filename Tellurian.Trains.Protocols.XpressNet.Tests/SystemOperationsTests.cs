using Tellurian.Trains.Protocols.XpressNet.Commands;
using Tellurian.Trains.Protocols.XpressNet.Notifications;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class SystemOperationsTests
{
    #region SetPowerUpModeCommand

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

    #endregion

    #region TransferErrorNotification

    [TestMethod]
    public void TransferErrorNotification_HasCorrectProperties()
    {
        var notification = new TransferErrorNotification();

        Assert.AreEqual(0x61, notification.Header);
        Assert.Contains("Transfer error", notification.Description);
    }

    [TestMethod]
    public void TransferErrorNotification_CreatedByFactory()
    {
        var buffer = new byte[] { 0x61, 0x80 };
        var notification = NotificationFactory.Create(buffer);

        Assert.IsInstanceOfType<TransferErrorNotification>(notification);
    }

    #endregion

    #region CommandStationBusyNotification

    [TestMethod]
    public void CommandStationBusyNotification_HasCorrectProperties()
    {
        var notification = new CommandStationBusyNotification();

        Assert.AreEqual(0x61, notification.Header);
        Assert.Contains("busy", notification.Description);
    }

    [TestMethod]
    public void CommandStationBusyNotification_CreatedByFactory()
    {
        var buffer = new byte[] { 0x61, 0x81 };
        var notification = NotificationFactory.Create(buffer);

        Assert.IsInstanceOfType<CommandStationBusyNotification>(notification);
    }

    #endregion
}
