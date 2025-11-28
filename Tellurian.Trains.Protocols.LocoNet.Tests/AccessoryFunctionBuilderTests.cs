using Tellurian.Trains.Interfaces.Accessories;

namespace Tellurian.Trains.Protocols.LocoNet.Tests;

[TestClass]
public class AccessoryFunctionBuilderTests
{
    [TestMethod]
    public void GetAccessoryFunctionBytes_CreatesBinaryCorrectly_WhenFlagsOff()
    {
        var actual = GetAccessoryFunctionBytes(AccessoryAddress.From(1), AccessoryInput.Port0, AccessoryFunction.ClosedOrGreen, OutputState.Off);
        Assert.AreEqual(0x04, actual[0]);
        Assert.AreEqual(0x00, actual[1]);
    }

    [TestMethod]
    public void GetAccessoryFunctionBytes_CreatesBinaryCorrectly_WhenFlagsOn()
    {
        var actual = GetAccessoryFunctionBytes(AccessoryAddress.From(511), AccessoryInput.Port3, AccessoryFunction.ThrownOrRed, OutputState.On);
        Assert.AreEqual(0x7F, actual[0]);
        Assert.AreEqual(0x3F, actual[1], "Byte 2");
    }

    public static byte[] GetAccessoryFunctionBytes(AccessoryAddress address, AccessoryInput input, AccessoryFunction function, OutputState state)
    {
        var result = new byte[2];
        result[0] = (byte)(((address.Number & 0x1F) << 2) + (byte)input);
        result[1] = (byte)(((address.Number >> 5) & 0x0F) + ((byte)function * 32) + ((byte)state * 16));
        return result;
    }
}
