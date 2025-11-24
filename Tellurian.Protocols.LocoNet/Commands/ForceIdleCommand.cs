namespace Tellurian.Trains.Protocols.LocoNet.Commands;
public sealed class ForceIdleCommand : Command    
{
    public const byte OperationCode = 0x84;
    public override byte[] GetBytesWithChecksum()
    {
        return AppendChecksum(OperationCode);
    }
}
