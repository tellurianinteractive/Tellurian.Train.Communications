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
        Assert.IsTrue(notification.Description.Contains("Transfer error"));
    }

    [TestMethod]
    public void TransferErrorNotification_CreatedByFactory()
    {
        var buffer = new byte[] { 0x61, 0x80 };
        var notification = NotificationFactory.Create(buffer);

        Assert.IsInstanceOfType(notification, typeof(TransferErrorNotification));
    }

    #endregion

    #region CommandStationBusyNotification

    [TestMethod]
    public void CommandStationBusyNotification_HasCorrectProperties()
    {
        var notification = new CommandStationBusyNotification();

        Assert.AreEqual(0x61, notification.Header);
        Assert.IsTrue(notification.Description.Contains("busy"));
    }

    [TestMethod]
    public void CommandStationBusyNotification_CreatedByFactory()
    {
        var buffer = new byte[] { 0x61, 0x81 };
        var notification = NotificationFactory.Create(buffer);

        Assert.IsInstanceOfType(notification, typeof(CommandStationBusyNotification));
    }

    #endregion

    #region PowerUpMode Enum

    [TestMethod]
    public void PowerUpMode_Manual_HasCorrectValue()
    {
        Assert.AreEqual(0, (int)PowerUpMode.Manual);
    }

    [TestMethod]
    public void PowerUpMode_Automatic_HasCorrectValue()
    {
        Assert.AreEqual(1, (int)PowerUpMode.Automatic);
    }

    #endregion
}
