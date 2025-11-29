using Tellurian.Trains.Interfaces.Accessories;

namespace Tellurian.Trains.Protocols.LocoNet.Tests;

[TestClass]
public class AccessoryFunctionBuilderTests
{
    [TestMethod]
    public void GetAccessoryFunctionBytes_CreatesBinaryCorrectly_WhenFlagsOff()
    {
        var actual = GetAccessoryFunctionBytes(Address.From(1), AccessoryInput.Port0, Position.ClosedOrGreen, MotorState.Off);
        Assert.AreEqual(0x04, actual[0]);
        Assert.AreEqual(0x00, actual[1]);
    }

    [TestMethod]
    public void GetAccessoryFunctionBytes_CreatesBinaryCorrectly_WhenFlagsOn()
    {
        var actual = GetAccessoryFunctionBytes(Address.From(511), AccessoryInput.Port3, Position.ThrownOrRed, MotorState.On);
        Assert.AreEqual(0x7F, actual[0]);
        Assert.AreEqual(0x3F, actual[1], "Byte 2");
    }

    internal static byte[] GetAccessoryFunctionBytes(Address address, AccessoryInput input, Position function, MotorState state)
    {
        var result = new byte[2];
        result[0] = (byte)(((address.Number & 0x1F) << 2) + (byte)input);
        result[1] = (byte)(((address.Number >> 5) & 0x0F) + ((byte)function * 32) + ((byte)state * 16));
        return result;
    }
}
