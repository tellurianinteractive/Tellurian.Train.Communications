using Tellurian.Trains.Protocols.LocoNet.Commands;

namespace Tellurian.Trains.Protocols.LocoNet.Tests;

[TestClass]
public class SetTurnoutCommandTests
{
    [TestMethod]
    public void SetTurnoutCommand_GeneratesCorrectOpcode_InBytes()
    {
        var command = new SetTurnoutCommand(
            new AccessoryAddress(0),
            AccessoryFunction.ClosedOrGreen,
            OutputState.Off);

        var bytes = command.GetBytesWithChecksum();

        Assert.AreEqual(0xB0, bytes[0]);
    }

    // ===== Factory Method Tests =====

    [TestMethod]
    public void Throw_CreatesCommand_WithThrownDirection()
    {
        var command = SetTurnoutCommand.Throw(new AccessoryAddress(100));

        Assert.AreEqual(AccessoryFunction.ThrownOrRed, command.Direction);
        Assert.AreEqual(OutputState.On, command.Output);
    }

    [TestMethod]
    public void Throw_WithActivateFalse_CreatesCommand_WithOutputOff()
    {
        var command = SetTurnoutCommand.Throw(new AccessoryAddress(100), activate: false);

        Assert.AreEqual(AccessoryFunction.ThrownOrRed, command.Direction);
        Assert.AreEqual(OutputState.Off, command.Output);
    }

    [TestMethod]
    public void Close_CreatesCommand_WithClosedDirection()
    {
        var command = SetTurnoutCommand.Close(new AccessoryAddress(100));

        Assert.AreEqual(AccessoryFunction.ClosedOrGreen, command.Direction);
        Assert.AreEqual(OutputState.On, command.Output);
    }

    [TestMethod]
    public void Close_WithActivateFalse_CreatesCommand_WithOutputOff()
    {
        var command = SetTurnoutCommand.Close(new AccessoryAddress(100), activate: false);

        Assert.AreEqual(AccessoryFunction.ClosedOrGreen, command.Direction);
        Assert.AreEqual(OutputState.Off, command.Output);
    }

    [TestMethod]
    public void TurnOff_CreatesCommand_WithOutputOff()
    {
        var command = SetTurnoutCommand.TurnOff(new AccessoryAddress(100));

        Assert.AreEqual(OutputState.Off, command.Output);
    }

    // ===== Address Encoding Tests =====

    [TestMethod]
    public void SetTurnoutCommand_EncodesAddress0_Correctly()
    {
        var command = SetTurnoutCommand.Throw(new AccessoryAddress(0));
        var bytes = command.GetBytesWithChecksum();

        // SW1 should have low 7 bits of address (0)
        Assert.AreEqual(0x00, bytes[1] & 0x7F, "SW1 - address low bits");
        // SW2 should have high 4 bits of address (0)
        Assert.AreEqual(0x00, bytes[2] & 0x0F, "SW2 - address high bits");
    }

    [TestMethod]
    public void SetTurnoutCommand_EncodesAddress127_Correctly()
    {
        var command = SetTurnoutCommand.Throw(new AccessoryAddress(127));
        var bytes = command.GetBytesWithChecksum();

        // SW1 should have low 7 bits = 127
        Assert.AreEqual(0x7F, bytes[1] & 0x7F, "SW1 - address low bits");
        // SW2 high 4 bits should be 0
        Assert.AreEqual(0x00, bytes[2] & 0x0F, "SW2 - address high bits");
    }

    [TestMethod]
    public void SetTurnoutCommand_EncodesAddress128_Correctly()
    {
        var command = SetTurnoutCommand.Throw(new AccessoryAddress(128));
        var bytes = command.GetBytesWithChecksum();

        // SW1 should have low 7 bits = 0 (128 & 0x7F)
        Assert.AreEqual(0x00, bytes[1] & 0x7F, "SW1 - address low bits");
        // SW2 high 4 bits should be 1 (128 >> 7)
        Assert.AreEqual(0x01, bytes[2] & 0x0F, "SW2 - address high bits");
    }

    [TestMethod]
    public void SetTurnoutCommand_EncodesAddress2047_Correctly()
    {
        var command = SetTurnoutCommand.Throw(new AccessoryAddress(2047));
        var bytes = command.GetBytesWithChecksum();

        // SW1 should have low 7 bits = 127 (2047 & 0x7F)
        Assert.AreEqual(0x7F, bytes[1] & 0x7F, "SW1 - address low bits");
        // SW2 high 4 bits should be 15 (2047 >> 7 = 15)
        Assert.AreEqual(0x0F, bytes[2] & 0x0F, "SW2 - address high bits");
    }

    // ===== Direction and Output Encoding Tests =====

    [TestMethod]
    public void SetTurnoutCommand_EncodesDirectionBit_ForClosedGreen()
    {
        var command = SetTurnoutCommand.Close(new AccessoryAddress(0));
        var bytes = command.GetBytesWithChecksum();

        // DIR bit (bit 5 of SW2) should be set for Closed/Green
        Assert.AreNotEqual(0, bytes[2] & 0x20, "DIR bit should be set");
    }

    [TestMethod]
    public void SetTurnoutCommand_EncodesDirectionBit_ForThrownRed()
    {
        var command = SetTurnoutCommand.Throw(new AccessoryAddress(0));
        var bytes = command.GetBytesWithChecksum();

        // DIR bit (bit 5 of SW2) should be clear for Thrown/Red
        Assert.AreEqual(0, bytes[2] & 0x20, "DIR bit should be clear");
    }

    [TestMethod]
    public void SetTurnoutCommand_EncodesOutputBit_WhenOn()
    {
        var command = SetTurnoutCommand.Throw(new AccessoryAddress(0), activate: true);
        var bytes = command.GetBytesWithChecksum();

        // ON bit (bit 4 of SW2) should be set
        Assert.AreNotEqual(0, bytes[2] & 0x10, "ON bit should be set");
    }

    [TestMethod]
    public void SetTurnoutCommand_EncodesOutputBit_WhenOff()
    {
        var command = SetTurnoutCommand.Throw(new AccessoryAddress(0), activate: false);
        var bytes = command.GetBytesWithChecksum();

        // ON bit (bit 4 of SW2) should be clear
        Assert.AreEqual(0, bytes[2] & 0x10, "ON bit should be clear");
    }
}

[TestClass]
public class SwitchAcknowledgeCommandTests
{
    [TestMethod]
    public void SwitchAcknowledgeCommand_GeneratesCorrectOpcode_InBytes()
    {
        var command = new SwitchAcknowledgeCommand(
            new AccessoryAddress(0),
            AccessoryFunction.ClosedOrGreen,
            OutputState.Off);

        var bytes = command.GetBytesWithChecksum();

        Assert.AreEqual(0xBD, bytes[0]);
    }

    [TestMethod]
    public void Throw_CreatesCommand_WithThrownDirection()
    {
        var command = SwitchAcknowledgeCommand.Throw(new AccessoryAddress(100));

        Assert.AreEqual(AccessoryFunction.ThrownOrRed, command.Direction);
        Assert.AreEqual(OutputState.On, command.Output);
    }

    [TestMethod]
    public void Close_CreatesCommand_WithClosedDirection()
    {
        var command = SwitchAcknowledgeCommand.Close(new AccessoryAddress(100));

        Assert.AreEqual(AccessoryFunction.ClosedOrGreen, command.Direction);
        Assert.AreEqual(OutputState.On, command.Output);
    }
}

[TestClass]
public class RequestSwitchStateCommandTests
{

    [TestMethod]
    public void RequestSwitchStateCommand_GeneratesCorrectOpcode_InBytes()
    {
        var command = new RequestSwitchStateCommand(new AccessoryAddress(0));
        var bytes = command.GetBytesWithChecksum();

        Assert.AreEqual(0xBC, bytes[0]);
    }

    [TestMethod]
    public void RequestSwitchStateCommand_EncodesAddressCorrectly()
    {
        var command = new RequestSwitchStateCommand(new AccessoryAddress(500));
        var bytes = command.GetBytesWithChecksum();

        // Decode address from bytes to verify
        ushort decodedAddress = (ushort)(bytes[1] | ((bytes[2] & 0x0F) << 7));
        Assert.AreEqual(500, decodedAddress);
    }

    [TestMethod]
    public void AllSwitchCommands_HaveDistinctOpcodes()
    {
        var opcodes = new[]
        {
            SetTurnoutCommand.OperationCode,
            SwitchAcknowledgeCommand.OperationCode,
            RequestSwitchStateCommand.OperationCode
        };

        // All opcodes should be unique
        Assert.AreEqual(3, opcodes.Distinct().Count(), "All switch command opcodes must be unique");
    }
}
