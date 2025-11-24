namespace Tellurian.Trains.Protocols.LocoNet.Commands;

public sealed class GetLocoAddressCommand(LocoAddress address) : Command
{
    public const byte OperationCode = 0xBF;
    public LocoAddress Address { get; private set; } = address;

    public override byte[] GetBytesWithChecksum()
    {
        return AppendChecksum([OperationCode, 0x00, Address.Low]);
    }
}
