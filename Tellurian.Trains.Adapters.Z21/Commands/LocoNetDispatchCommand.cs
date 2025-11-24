namespace Tellurian.Trains.Adapters.Z21;

/// <summary>
/// Dispatches a loco address for pick-up by a handset.
/// </summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 9.4
/// </remarks>
public sealed class LocoNetDispatchCommand : Command
{
    private readonly short Address;
    public LocoNetDispatchCommand(short address)
    {
        Address = address;
    }

    internal override Frame ToFrame()
    {
        return new Frame(FrameHeader.LocoNetDispatch, BitConverter.GetBytes(Address));
    }
}
