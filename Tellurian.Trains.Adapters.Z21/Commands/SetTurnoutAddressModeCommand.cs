namespace Tellurian.Trains.Adapters.Z21;

public class SetTurnoutAddressModeCommand : SetAddressModeCommand
{
    public SetTurnoutAddressModeCommand(short address, AddressMode mode) : base(address, mode) { }

    internal override Frame ToFrame()
    {
        return new Frame(FrameHeader.SetTurnoutAddressMode, GetData());
    }
}
