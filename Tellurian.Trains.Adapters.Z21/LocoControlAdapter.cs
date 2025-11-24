using Tellurian.Trains.Interfaces.Locos;
using Tellurian.Trains.Protocols.XpressNet;
using Tellurian.Trains.Protocols.XpressNet.Commands;

namespace Tellurian.Trains.Adapters.Z21;

public sealed partial class Adapter : ILocoControl
{
    public bool SetFunction(Interfaces.Locos.LocoAddress address, LocoFunction function)
    {
        return Send(new LocoFunctionCommand(address.Map(), (byte)function.Number, function.IsOn ? LocoFunctionStates.On : LocoFunctionStates.Off));
    }

    public bool EmergencyStop(Interfaces.Locos.LocoAddress address)
    {
        return Send(new LocoEmergencyStopCommand(address.Map()));
    }

    public bool Drive(Interfaces.Locos.LocoAddress address, LocoDrive drive)
    {
        return Send(new LocoDriveCommand(address.Map(), drive.Speed.Map(), drive.Direction.Map()));
    }
}
