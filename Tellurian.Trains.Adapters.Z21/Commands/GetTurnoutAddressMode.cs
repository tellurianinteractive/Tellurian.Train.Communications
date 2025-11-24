namespace Tellurian.Trains.Adapters.Z21;

/// <summary>
/// Requests Z21 to send response <see cref="TurnoutAddressMode"/>
/// </summary>
public class GetTurnoutAddressMode : Command
{
    private readonly short Address;
    internal override Frame ToFrame()
    {
        return new Frame(FrameHeader.TurnoutAddressMode, BitConverterExtensions.GetBigEndianBytes(Address));
    }
    public GetTurnoutAddressMode(short address)
    {
        Address = address;
    }
}
