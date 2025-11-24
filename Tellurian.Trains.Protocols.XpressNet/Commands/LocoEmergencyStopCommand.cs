namespace Tellurian.Trains.Protocols.XpressNet.Commands;

public sealed class LocoEmergencyStopCommand : Command {
    public LocoEmergencyStopCommand(LocoAddress address) : base(0x90, GetData(address)) { }

    private static byte[] GetData(LocoAddress address) {
        return address.GetBytesAccordingToXpressNet();
    }
}
