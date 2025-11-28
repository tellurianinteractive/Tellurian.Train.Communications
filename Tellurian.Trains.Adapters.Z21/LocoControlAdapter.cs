using Tellurian.Trains.Interfaces.Locos;
using Tellurian.Trains.Protocols.XpressNet;
using Tellurian.Trains.Protocols.XpressNet.Commands;

namespace Tellurian.Trains.Adapters.Z21;

public sealed partial class Adapter : ILocoControl
{
    public Task<bool> SetFunctionAsync(Interfaces.Locos.LocoAddress address, LocoFunction function, CancellationToken cancellationToken = default)
    {
        return SendAsync(new LocoFunctionCommand(address, (byte)function.Number, function.IsOn ? LocoFunctionStates.On : LocoFunctionStates.Off), cancellationToken);
    }

    public Task<bool> EmergencyStopAsync(Interfaces.Locos.LocoAddress address, CancellationToken cancellationToken = default)
    {
        return SendAsync(new LocoEmergencyStopCommand(address), cancellationToken);
    }

    public Task<bool> DriveAsync(Interfaces.Locos.LocoAddress address, LocoDrive drive, CancellationToken cancellationToken = default)
    {
        return SendAsync(new LocoDriveCommand(address, drive.Speed.Map(), drive.Direction.Map()), cancellationToken);
    }
}
