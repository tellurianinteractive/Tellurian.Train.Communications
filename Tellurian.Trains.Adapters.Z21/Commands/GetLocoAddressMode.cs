namespace Tellurian.Trains.Adapters.Z21;

/// <summary>
/// Requests Z21 for a <see cref="LocoAddressModeNotification"/>
/// </summary>
public class GetLocoAddressMode : Command
{
    private readonly short Address;
    public GetLocoAddressMode(short address)
    {
        Address = address;
    }
    internal override Frame ToFrame()
    {
        return new Frame(FrameHeader.LocoAddressMode, BitConverterExtensions.GetBigEndianBytes(Address));
    }
}
