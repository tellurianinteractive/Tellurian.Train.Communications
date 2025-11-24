namespace Tellurian.Trains.Adapters.Z21;

/// <summary>
/// Sets <see cref="AddressMode"/> for a specific loco address.
/// </summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 3.2
/// </remarks>
public class SetLocoAddressModeCommand : SetAddressModeCommand
{
    public SetLocoAddressModeCommand(short address, AddressMode mode) : base(address, mode) { }
    internal override Frame ToFrame()
    {
        return new Frame(FrameHeader.SetLocomotiveAddressMode, GetData());
    }
}
