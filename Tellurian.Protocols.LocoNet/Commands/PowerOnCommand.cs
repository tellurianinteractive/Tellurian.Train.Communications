namespace Tellurian.Trains.Protocols.LocoNet.Commands;

public sealed class PowerOnCommand : Command
{
    public const byte OperationCode = 0x83;
    public override byte[] GetBytesWithChecksum()
    {
        return AppendChecksum(OperationCode);
    }
}
