namespace Tellurian.Trains.Protocols.LocoNet.Commands;

/// <summary>
/// Force IDLE state - Emergency stop all locomotives.
/// OPC_IDLE (0x85): Broadcasts emergency stop to all active locomotives.
/// </summary>
public sealed class ForceIdleCommand : Command
{
    public const byte OperationCode = 0x85;
    public override byte[] GetBytesWithChecksum()
    {
        return AppendChecksum(OperationCode);
    }
}
