using Tellurian.Trains.Communications.Interfaces.Accessories;
using Tellurian.Trains.Protocols.LocoNet.Commands;

namespace Tellurian.Trains.Protocols.LocoNet.Tests;

[TestClass]
public class SetTurnoutCommandTests
{
    [TestMethod]
    public void SetTurnoutCommand_GeneratesCorrectOpcode_InBytes()
    {
        var command = new SetTurnoutCommand(
            Address.From(1),
            Position.ClosedOrGreen,
            MotorState.Off);

        var bytes = command.GetBytesWithChecksum();

        Assert.AreEqual(0xB0, bytes[0]);
    }

    // ===== Factory Method Tests =====

    [TestMethod]
    public void Throw_CreatesCommand_WithThrownDirection()
    {
        var command = SetTurnoutCommand.Throw(Address.From(100));

        Assert.AreEqual(Position.ThrownOrRed, command.Direction);
        Assert.AreEqual(MotorState.On, command.Output);
    }

    [TestMethod]
    public void Throw_WithActivateFalse_CreatesCommand_WithOutputOff()
    {
        var command = SetTurnoutCommand.Throw(Address.From(100), activate: false);

        Assert.AreEqual(Position.ThrownOrRed, command.Direction);
        Assert.AreEqual(MotorState.Off, command.Output);
    }

    [TestMethod]
    public void Close_CreatesCommand_WithClosedDirection()
    {
        var command = SetTurnoutCommand.Close(Address.From(100));

        Assert.AreEqual(Position.ClosedOrGreen, command.Direction);
        Assert.AreEqual(MotorState.On, command.Output);
    }

    [TestMethod]
    public void Close_WithActivateFalse_CreatesCommand_WithOutputOff()
    {
        var command = SetTurnoutCommand.Close(Address.From(100), activate: false);

        Assert.AreEqual(Position.ClosedOrGreen, command.Direction);
        Assert.AreEqual(MotorState.Off, command.Output);
    }

    [TestMethod]
    public void TurnOff_CreatesCommand_WithOutputOff()
    {
        var command = SetTurnoutCommand.TurnOff(Address.From(100));

        Assert.AreEqual(MotorState.Off, command.Output);
    }

    // ===== Address Encoding Tests =====
    // Note: LocoNet uses 1-based user addressing. User address N encodes as wire address N-1.

    [TestMethod]
    public void SetTurnoutCommand_EncodesAddress1_Correctly()
    {
        // User address 1 → wire address 0
        var command = SetTurnoutCommand.Throw(Address.From(1));
        var bytes = command.GetBytesWithChecksum();

        // SW1 should have low 7 bits of wire address (0)
        Assert.AreEqual(0x00, bytes[1] & 0x7F, "SW1 - address low bits");
        // SW2 should have high 4 bits of wire address (0)
        Assert.AreEqual(0x00, bytes[2] & 0x0F, "SW2 - address high bits");
    }

    [TestMethod]
    public void SetTurnoutCommand_EncodesAddress128_Correctly()
    {
        // User address 128 → wire address 127
        var command = SetTurnoutCommand.Throw(Address.From(128));
        var bytes = command.GetBytesWithChecksum();

        // SW1 should have low 7 bits = 127
        Assert.AreEqual(0x7F, bytes[1] & 0x7F, "SW1 - address low bits");
        // SW2 high 4 bits should be 0
        Assert.AreEqual(0x00, bytes[2] & 0x0F, "SW2 - address high bits");
    }

    [TestMethod]
    public void SetTurnoutCommand_EncodesAddress129_Correctly()
    {
        // User address 129 → wire address 128
        var command = SetTurnoutCommand.Throw(Address.From(129));
        var bytes = command.GetBytesWithChecksum();

        // SW1 should have low 7 bits = 0 (128 & 0x7F)
        Assert.AreEqual(0x00, bytes[1] & 0x7F, "SW1 - address low bits");
        // SW2 high 4 bits should be 1 (128 >> 7)
        Assert.AreEqual(0x01, bytes[2] & 0x0F, "SW2 - address high bits");
    }

    [TestMethod]
    public void SetTurnoutCommand_EncodesAddress2047_Correctly()
    {
        // User address 2047 → wire address 2046
        var command = SetTurnoutCommand.Throw(Address.From(2047));
        var bytes = command.GetBytesWithChecksum();

        // SW1 should have low 7 bits = 126 (2046 & 0x7F)
        Assert.AreEqual(0x7E, bytes[1] & 0x7F, "SW1 - address low bits");
        // SW2 high 4 bits should be 15 (2046 >> 7 = 15)
        Assert.AreEqual(0x0F, bytes[2] & 0x0F, "SW2 - address high bits");
    }

    // ===== Direction and Output Encoding Tests =====

    [TestMethod]
    public void SetTurnoutCommand_EncodesDirectionBit_ForClosedGreen()
    {
        var command = SetTurnoutCommand.Close(Address.From(1));
        var bytes = command.GetBytesWithChecksum();

        // DIR bit (bit 5 of SW2) should be set for Closed/Green
        Assert.AreNotEqual(0, bytes[2] & 0x20, "DIR bit should be set");
    }

    [TestMethod]
    public void SetTurnoutCommand_EncodesDirectionBit_ForThrownRed()
    {
        var command = SetTurnoutCommand.Throw(Address.From(1));
        var bytes = command.GetBytesWithChecksum();

        // DIR bit (bit 5 of SW2) should be clear for Thrown/Red
        Assert.AreEqual(0, bytes[2] & 0x20, "DIR bit should be clear");
    }

    [TestMethod]
    public void SetTurnoutCommand_EncodesOutputBit_WhenOn()
    {
        var command = SetTurnoutCommand.Throw(Address.From(1), activate: true);
        var bytes = command.GetBytesWithChecksum();

        // ON bit (bit 4 of SW2) should be set
        Assert.AreNotEqual(0, bytes[2] & 0x10, "ON bit should be set");
    }

    [TestMethod]
    public void SetTurnoutCommand_EncodesOutputBit_WhenOff()
    {
        var command = SetTurnoutCommand.Throw(Address.From(1), activate: false);
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
            Address.From(1),
            Position.ClosedOrGreen,
            MotorState.Off);

        var bytes = command.GetBytesWithChecksum();

        Assert.AreEqual(0xBD, bytes[0]);
    }

    [TestMethod]
    public void Throw_CreatesCommand_WithThrownDirection()
    {
        var command = SwitchAcknowledgeCommand.Throw(Address.From(100));

        Assert.AreEqual(Position.ThrownOrRed, command.Direction);
        Assert.AreEqual(MotorState.On, command.Output);
    }

    [TestMethod]
    public void Close_CreatesCommand_WithClosedDirection()
    {
        var command = SwitchAcknowledgeCommand.Close(Address.From(100));

        Assert.AreEqual(Position.ClosedOrGreen, command.Direction);
        Assert.AreEqual(MotorState.On, command.Output);
    }
}

[TestClass]
public class RequestSwitchStateCommandTests
{

    [TestMethod]
    public void RequestSwitchStateCommand_GeneratesCorrectOpcode_InBytes()
    {
        var command = new RequestSwitchStateCommand(Address.From(1));
        var bytes = command.GetBytesWithChecksum();

        Assert.AreEqual(0xBC, bytes[0]);
    }

    [TestMethod]
    public void RequestSwitchStateCommand_EncodesAddressCorrectly()
    {
        // User address 500 → wire address 499
        var command = new RequestSwitchStateCommand(Address.From(500));
        var bytes = command.GetBytesWithChecksum();

        // Decode wire address from bytes to verify encoding
        ushort wireAddress = (ushort)(bytes[1] | ((bytes[2] & 0x0F) << 7));
        Assert.AreEqual(499, wireAddress, "User address 500 should encode to wire address 499");
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
