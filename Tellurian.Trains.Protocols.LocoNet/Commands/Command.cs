namespace Tellurian.Trains.Protocols.LocoNet.Commands;

public abstract class Command : Message
{
    public virtual byte[] GetBytesWithChecksum() { return []; }
}
