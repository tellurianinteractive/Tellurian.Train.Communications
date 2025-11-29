using Tellurian.Trains.Protocols.XpressNet.Decoder;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class ProgramOnMainWriteBitCommandTests
{
    [TestMethod]
    public void ProgramOnMainWriteBit_ReturnsCorrectBytes_ForBit0SetTo1()
    {
        var target = new ProgramOnMainWriteBitCommand(Address.From(3), 29, 0, true);
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
        var target = new ProgramOnMainWriteBitCommand(Address.From(3), 29, 7, false);
        var data = target.GetData();

        Assert.AreEqual(0xE8, data[4]);
        Assert.AreEqual(0x1C, data[5]);
        Assert.AreEqual(0xF7, data[6]);
    }

    [TestMethod]
    public void ProgramOnMainWriteBit_ReturnsCorrectBytes_ForBit3SetTo1()
    {
        var target = new ProgramOnMainWriteBitCommand(Address.From(100), 1, 3, true);
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
        var target = new ProgramOnMainWriteBitCommand(Address.From(1000), 513, 5, false);
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
            _ = new ProgramOnMainWriteBitCommand(Address.From(3), 29, 8, true);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    [TestMethod]
    public void ProgramOnMainWriteBit_Throws_WhenCVIsZero()
    {
        try
        {
            _ = new ProgramOnMainWriteBitCommand(Address.From(3), 0, 0, true);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    [TestMethod]
    public void ProgramOnMainWriteBit_Throws_WhenCVIsTooHigh()
    {
        try
        {
            _ = new ProgramOnMainWriteBitCommand(Address.From(3), 1025, 0, true);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }
}
