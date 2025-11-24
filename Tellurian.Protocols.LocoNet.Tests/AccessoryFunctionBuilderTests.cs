namespace Tellurian.Trains.Protocols.LocoNet.Tests;

[TestClass]
public class AccessoryFunctionBuilderTests
{
    [TestMethod]
    public void CreatesBinaryWithFlagsOff()
    {
        var actual = GetAccessoryFunctionBytes(new AccessoryAddress(1, AccessoryInput.Port0), AccessoryFunction.ClosedOrGreen, OutputState.Off);
        Assert.AreEqual(0x04, actual[0]);
        Assert.AreEqual(0x00, actual[1]);
    }
    [TestMethod]
    public void CreatesBinaryWithFlagsOn()
    {
        var actual = GetAccessoryFunctionBytes(new AccessoryAddress(511, AccessoryInput.Port3), AccessoryFunction.ThrownOrRed, OutputState.On);
        Assert.AreEqual(0x7F, actual[0]);
        Assert.AreEqual(0x3F, actual[1], "Byte 2");
    }


    public static byte[] GetAccessoryFunctionBytes(AccessoryAddress address, AccessoryFunction function, OutputState state)
    {
        var result = new byte[2];
        result[0] = (byte)(((address.Value & 0x1F) << 2) + (byte)address.Input);
        result[1] = (byte)(((address.Value >> 5) & 0x0F) + ((byte)function * 32) + ((byte)state * 16));
        return result;
    }

}
