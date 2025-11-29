using Tellurian.Trains.Interfaces.Locos;

namespace Tellurian.Trains.Protocols.XpressNet.Commands;

public sealed class LocoEmergencyStopCommand : Command
{
    public LocoEmergencyStopCommand(Address address) : base(0x90, GetData(address)) { }

    private static byte[] GetData(Address address)
    {
        return address.GetBytesAccordingToXpressNet();
    }
}
