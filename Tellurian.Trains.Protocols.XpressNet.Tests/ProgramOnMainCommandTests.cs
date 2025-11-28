using Tellurian.Trains.Interfaces.Decoder;
using Tellurian.Trains.Protocols.XpressNet.Decoder;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class ProgramOnMainCommandTests
{
    #region ProgramOnMain Byte Write

    [TestMethod]
    public void ProgramOnMainWriteByte_ReturnsCorrectBytes_ForShortAddressAndCV1()
    {
        var target = new ProgramOnMainWriteByteCommand(new LocoAddress(3), new CV(1, 0x55));
        var data = target.GetData();

        Assert.AreEqual(0xE6, data[0]);
        Assert.AreEqual(0x30, data[1]);
        Assert.AreEqual(0x00, data[2]);
        Assert.AreEqual(0x03, data[3]);
        Assert.AreEqual(0xEC, data[4]);
        Assert.AreEqual(0x00, data[5]);
        Assert.AreEqual(0x55, data[6]);
    }

    [TestMethod]
    public void ProgramOnMainWriteByte_ReturnsCorrectBytes_ForLongAddressAndCV256()
    {
        var target = new ProgramOnMainWriteByteCommand(new LocoAddress(9999), new CV(256, 0xAA));
        var data = target.GetData();

        Assert.AreEqual(0xE6, data[0]);
        Assert.AreEqual(0x30, data[1]);
        Assert.AreEqual(0xE7, data[2]);
        Assert.AreEqual(0x0F, data[3]);
        Assert.AreEqual(0xEC, data[4]);
        Assert.AreEqual(0xFF, data[5]);
        Assert.AreEqual(0xAA, data[6]);
    }

    [TestMethod]
    public void ProgramOnMainWriteByte_ReturnsCorrectBytes_ForCV257()
    {
        var target = new ProgramOnMainWriteByteCommand(new LocoAddress(3), new CV(257, 0x42));
        var data = target.GetData();

        Assert.AreEqual(0xED, data[4]);
        Assert.AreEqual(0x00, data[5]);
        Assert.AreEqual(0x42, data[6]);
    }

    [TestMethod]
    public void ProgramOnMainWriteByte_ReturnsCorrectBytes_ForCV513()
    {
        var target = new ProgramOnMainWriteByteCommand(new LocoAddress(3), new CV(513, 0x77));
        var data = target.GetData();

        Assert.AreEqual(0xEE, data[4]);
        Assert.AreEqual(0x00, data[5]);
        Assert.AreEqual(0x77, data[6]);
    }

    [TestMethod]
    public void ProgramOnMainWriteByte_ReturnsCorrectBytes_ForCV769()
    {
        var target = new ProgramOnMainWriteByteCommand(new LocoAddress(3), new CV(769, 0x99));
        var data = target.GetData();

        Assert.AreEqual(0xEF, data[4]);
        Assert.AreEqual(0x00, data[5]);
        Assert.AreEqual(0x99, data[6]);
    }

    [TestMethod]
    public void ProgramOnMainWriteByte_ReturnsCorrectBytes_ForCV1024()
    {
        var target = new ProgramOnMainWriteByteCommand(new LocoAddress(3), new CV(1024, 0xBB));
        var data = target.GetData();

        Assert.AreEqual(0xEF, data[4]);
        Assert.AreEqual(0xFF, data[5]);
        Assert.AreEqual(0xBB, data[6]);
    }

    [TestMethod]
    public void ProgramOnMainWriteByte_Throws_WhenCVIsZero()
    {
        try
        {
            _ = new CV(0, 0x00);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    [TestMethod]
    public void ProgramOnMainWriteByte_Throws_WhenCVIsTooHigh()
    {
        try
        {
            _ = new CV(1025, 0x00);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    #endregion

    #region ProgramOnMain Bit Write

    [TestMethod]
    public void ProgramOnMainWriteBit_ReturnsCorrectBytes_ForBit0SetTo1()
    {
        var target = new ProgramOnMainWriteBitCommand(new LocoAddress(3), 29, 0, true);
        var data = target.GetData();

        Assert.AreEqual(0xE6, data[0]);
        Assert.AreEqual(0x30, data[1]);
        Assert.AreEqual(0x00, data[2]);
        Assert.AreEqual(0x03, data[3]);
        Assert.AreEqual(0xE8, data[4]);
        Assert.AreEqual(0x1C, data[5]);
        Assert.AreEqual(0xF8, data[6]);
    }

    [TestMethod]
    public void ProgramOnMainWriteBit_ReturnsCorrectBytes_ForBit7SetTo0()
    {
        var target = new ProgramOnMainWriteBitCommand(new LocoAddress(3), 29, 7, false);
        var data = target.GetData();

        Assert.AreEqual(0xE8, data[4]);
        Assert.AreEqual(0x1C, data[5]);
        Assert.AreEqual(0xF7, data[6]);
    }

    [TestMethod]
    public void ProgramOnMainWriteBit_ReturnsCorrectBytes_ForBit3SetTo1()
    {
        var target = new ProgramOnMainWriteBitCommand(new LocoAddress(100), 1, 3, true);
        var data = target.GetData();

        Assert.AreEqual(0x00, data[2]);
        Assert.AreEqual(0x64, data[3]);
        Assert.AreEqual(0xE8, data[4]);
        Assert.AreEqual(0x00, data[5]);
        Assert.AreEqual(0xFB, data[6]);
    }

    [TestMethod]
    public void ProgramOnMainWriteBit_ReturnsCorrectBytes_ForLongAddressAndHighCV()
    {
        var target = new ProgramOnMainWriteBitCommand(new LocoAddress(1000), 513, 5, false);
        var data = target.GetData();

        Assert.AreEqual(0xC3, data[2]);
        Assert.AreEqual(0xE8, data[3]);
        Assert.AreEqual(0xEA, data[4]);
        Assert.AreEqual(0x00, data[5]);
        Assert.AreEqual(0xF5, data[6]);
    }

    [TestMethod]
    public void ProgramOnMainWriteBit_Throws_WhenBitPositionTooHigh()
    {
        try
        {
            _ = new ProgramOnMainWriteBitCommand(new LocoAddress(3), 29, 8, true);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    [TestMethod]
    public void ProgramOnMainWriteBit_Throws_WhenCVIsZero()
    {
        try
        {
            _ = new ProgramOnMainWriteBitCommand(new LocoAddress(3), 0, 0, true);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    [TestMethod]
    public void ProgramOnMainWriteBit_Throws_WhenCVIsTooHigh()
    {
        try
        {
            _ = new ProgramOnMainWriteBitCommand(new LocoAddress(3), 1025, 0, true);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    #endregion
}
