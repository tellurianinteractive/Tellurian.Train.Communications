namespace Tellurian.Trains.Adapters.Z21;

/// <summary>
/// Wraps a raw LocoNet message (including checksum) in a Z21 frame for transmission.
/// </summary>
public sealed class LocoNetRawCommand : Command
{
    private readonly byte[] _locoNetMessage;

    public LocoNetRawCommand(byte[] locoNetMessage)
    {
        _locoNetMessage = locoNetMessage ?? throw new ArgumentNullException(nameof(locoNetMessage));
    }

    internal override Frame ToFrame() => new(FrameHeader.LocoNetCommand, _locoNetMessage);
}
