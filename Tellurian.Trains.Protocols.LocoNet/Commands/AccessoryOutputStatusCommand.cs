using Tellurian.Trains.Communications.Interfaces.Accessories;

namespace Tellurian.Trains.Protocols.LocoNet.Commands;

/// <summary>
/// OPC_SW_REP (0xB1) - Accessory output status report.
/// Reports the current output state of an accessory decoder.
/// Bit 6 of SN2 is 0 to indicate output status (not input feedback).
/// Bit 5: Closed output ON. Bit 4: Thrown output ON.
/// </summary>
public sealed class AccessoryOutputStatusCommand : Command
{
    public const byte OperationCode = 0xB1;

    public AccessoryOutputStatusCommand(Address address, bool closedOutputOn, bool thrownOutputOn)
    {
        Address = address;
        ClosedOutputOn = closedOutputOn;
        ThrownOutputOn = thrownOutputOn;
    }

    public Address Address { get; }
    public bool ClosedOutputOn { get; }
    public bool ThrownOutputOn { get; }

    public static AccessoryOutputStatusCommand Closed(Address address) =>
        new(address, closedOutputOn: true, thrownOutputOn: false);

    public static AccessoryOutputStatusCommand Thrown(Address address) =>
        new(address, closedOutputOn: false, thrownOutputOn: true);

    public override byte[] GetBytesWithChecksum()
    {
        byte sw1 = (byte)(Address.WireAddress & 0x7F);
        byte sw2 = (byte)((Address.WireAddress >> 7) & 0x0F);

        // Bit 6 = 0 for output status (already 0)
        if (ClosedOutputOn)
            sw2 |= 0x20;
        if (ThrownOutputOn)
            sw2 |= 0x10;

        return AppendChecksum([OperationCode, sw1, sw2]);
    }
}
