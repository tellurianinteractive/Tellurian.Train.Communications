using Tellurian.Trains.Communications.Interfaces.Locos;
using Tellurian.Trains.Protocols.XpressNet;
using Tellurian.Trains.Protocols.XpressNet.Commands;

namespace Tellurian.Trains.Adapters.Z21;

public sealed partial class Adapter : ILoco
{
    public Task<bool> SetFunctionAsync(Tellurian.Trains.Communications.Interfaces.Locos.Address address, Function function, CancellationToken cancellationToken = default)
    {
        return SendAsync(new LocoFunctionCommand(address, (byte)function.Number, function.IsOn ? LocoFunctionStates.On : LocoFunctionStates.Off), cancellationToken);
    }

    public Task<bool> EmergencyStopAsync(Tellurian.Trains.Communications.Interfaces.Locos.Address address, CancellationToken cancellationToken = default)
    {
        return SendAsync(new LocoEmergencyStopCommand(address), cancellationToken);
    }

    public Task<bool> DriveAsync(Tellurian.Trains.Communications.Interfaces.Locos.Address address, Drive drive, CancellationToken cancellationToken = default)
    {
        return SendAsync(new LocoDriveCommand(address, drive.Speed.Map(), drive.Direction.Map()), cancellationToken);
    }
}
