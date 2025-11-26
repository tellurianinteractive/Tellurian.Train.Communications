using Tellurian.Trains.Protocols.XpressNet.Decoder;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class ServiceModeCommandTests
{
    #region Register Mode Read

    [TestMethod]
    public void RegisterModeRead_ReturnsCorrectBytes_ForRegister1()
    {
        var target = new ServiceModeReadRegisterCommand(1);
        var data = target.GetData();
        Assert.AreEqual(0x22, data[0]); // Header with length 2
        Assert.AreEqual(0x11, data[1]); // Identification
        Assert.AreEqual(0x01, data[2]); // Register 1
    }

    [TestMethod]
    public void RegisterModeRead_ReturnsCorrectBytes_ForRegister8()
    {
        var target = new ServiceModeReadRegisterCommand(8);
        var data = target.GetData();
        Assert.AreEqual(0x22, data[0]);
        Assert.AreEqual(0x11, data[1]);
        Assert.AreEqual(0x08, data[2]);
    }

    [TestMethod]
    public void RegisterModeRead_Throws_WhenRegisterIsZero()
    {
        try
        {
            _ = new ServiceModeReadRegisterCommand(0);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    [TestMethod]
    public void RegisterModeRead_Throws_WhenRegisterIsTooHigh()
    {
        try
        {
            _ = new ServiceModeReadRegisterCommand(9);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    #endregion

    #region Direct Mode Read

    [TestMethod]
    public void DirectModeRead_ReturnsCorrectBytes_ForCV1()
    {
        var target = new ServiceModeReadDirectCommand(1);
        var data = target.GetData();
        Assert.AreEqual(0x22, data[0]); // Header with length 2
        Assert.AreEqual(0x15, data[1]); // Identification for Direct Mode
        Assert.AreEqual(0x01, data[2]); // CV 1
    }

    [TestMethod]
    public void DirectModeRead_ReturnsCorrectBytes_ForCV256()
    {
        var target = new ServiceModeReadDirectCommand(256);
        var data = target.GetData();
        Assert.AreEqual(0x22, data[0]);
        Assert.AreEqual(0x15, data[1]);
        Assert.AreEqual(0x00, data[2]); // CV 256 is encoded as 0x00
    }

    [TestMethod]
    public void DirectModeRead_ReturnsCorrectBytes_ForCV128()
    {
        var target = new ServiceModeReadDirectCommand(128);
        var data = target.GetData();
        Assert.AreEqual(0x22, data[0]);
        Assert.AreEqual(0x15, data[1]);
        Assert.AreEqual(0x80, data[2]);
    }

    [TestMethod]
    public void DirectModeRead_Throws_WhenCVIsZero()
    {
        try
        {
            _ = new ServiceModeReadDirectCommand(0);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    [TestMethod]
    public void DirectModeRead_Throws_WhenCVIsTooHigh()
    {
        try
        {
            _ = new ServiceModeReadDirectCommand(257);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    #endregion

    #region Paged Mode Read

    [TestMethod]
    public void PagedModeRead_ReturnsCorrectBytes_ForCV1()
    {
        var target = new ServiceModeReadPagedCommand(1);
        var data = target.GetData();
        Assert.AreEqual(0x22, data[0]); // Header with length 2
        Assert.AreEqual(0x14, data[1]); // Identification for Paged Mode
        Assert.AreEqual(0x01, data[2]); // CV 1
    }

    [TestMethod]
    public void PagedModeRead_ReturnsCorrectBytes_ForCV256()
    {
        var target = new ServiceModeReadPagedCommand(256);
        var data = target.GetData();
        Assert.AreEqual(0x22, data[0]);
        Assert.AreEqual(0x14, data[1]);
        Assert.AreEqual(0x00, data[2]); // CV 256 is encoded as 0x00
    }

    #endregion

    #region Register Mode Write

    [TestMethod]
    public void RegisterModeWrite_ReturnsCorrectBytes_ForRegister1()
    {
        var target = new ServiceModeWriteRegisterCommand(1, 0x55);
        var data = target.GetData();
        Assert.AreEqual(0x23, data[0]); // Header with length 3
        Assert.AreEqual(0x12, data[1]); // Identification
        Assert.AreEqual(0x01, data[2]); // Register 1
        Assert.AreEqual(0x55, data[3]); // Value
    }

    [TestMethod]
    public void RegisterModeWrite_ReturnsCorrectBytes_ForRegister8()
    {
        var target = new ServiceModeWriteRegisterCommand(8, 0xAA);
        var data = target.GetData();
        Assert.AreEqual(0x23, data[0]);
        Assert.AreEqual(0x12, data[1]);
        Assert.AreEqual(0x08, data[2]);
        Assert.AreEqual(0xAA, data[3]);
    }

    [TestMethod]
    public void RegisterModeWrite_Throws_WhenRegisterIsZero()
    {
        try
        {
            _ = new ServiceModeWriteRegisterCommand(0, 0x00);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    [TestMethod]
    public void RegisterModeWrite_Throws_WhenRegisterIsTooHigh()
    {
        try
        {
            _ = new ServiceModeWriteRegisterCommand(9, 0x00);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    #endregion

    #region Direct Mode Write

    [TestMethod]
    public void DirectModeWrite_ReturnsCorrectBytes_ForCV1()
    {
        var target = new ServiceModeWriteDirectCommand(1, 0x03);
        var data = target.GetData();
        Assert.AreEqual(0x23, data[0]); // Header with length 3
        Assert.AreEqual(0x16, data[1]); // Identification for Direct Mode
        Assert.AreEqual(0x01, data[2]); // CV 1
        Assert.AreEqual(0x03, data[3]); // Value
    }

    [TestMethod]
    public void DirectModeWrite_ReturnsCorrectBytes_ForCV256()
    {
        var target = new ServiceModeWriteDirectCommand(256, 0xFF);
        var data = target.GetData();
        Assert.AreEqual(0x23, data[0]);
        Assert.AreEqual(0x16, data[1]);
        Assert.AreEqual(0x00, data[2]); // CV 256 is encoded as 0x00
        Assert.AreEqual(0xFF, data[3]);
    }

    [TestMethod]
    public void DirectModeWrite_Throws_WhenCVIsZero()
    {
        try
        {
            _ = new ServiceModeWriteDirectCommand(0, 0x00);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    [TestMethod]
    public void DirectModeWrite_Throws_WhenCVIsTooHigh()
    {
        try
        {
            _ = new ServiceModeWriteDirectCommand(257, 0x00);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    #endregion

    #region Paged Mode Write

    [TestMethod]
    public void PagedModeWrite_ReturnsCorrectBytes_ForCV1()
    {
        var target = new ServiceModeWritePagedCommand(1, 0x07);
        var data = target.GetData();
        Assert.AreEqual(0x23, data[0]); // Header with length 3
        Assert.AreEqual(0x17, data[1]); // Identification for Paged Mode
        Assert.AreEqual(0x01, data[2]); // CV 1
        Assert.AreEqual(0x07, data[3]); // Value
    }

    [TestMethod]
    public void PagedModeWrite_ReturnsCorrectBytes_ForCV256()
    {
        var target = new ServiceModeWritePagedCommand(256, 0x42);
        var data = target.GetData();
        Assert.AreEqual(0x23, data[0]);
        Assert.AreEqual(0x17, data[1]);
        Assert.AreEqual(0x00, data[2]); // CV 256 is encoded as 0x00
        Assert.AreEqual(0x42, data[3]);
    }

    #endregion

    #region Service Mode Results Command

    [TestMethod]
    public void ServiceModeResults_ReturnsCorrectBytes()
    {
        var target = new ServiceModeResultsCommand();
        var data = target.GetData();
        Assert.AreEqual(0x21, data[0]); // Header with length 1
        Assert.AreEqual(0x10, data[1]); // Identification
    }

    #endregion
}
