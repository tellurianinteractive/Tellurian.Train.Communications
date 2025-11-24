namespace Tellurian.Trains.Protocols.LocoNet.Commands;

public sealed class PowerOffCommand : Command
{
    public const byte OperationCode = 0x82;
    public override byte[] GetBytesWithChecksum()
    {
        return AppendChecksum(OperationCode);
    }
}
