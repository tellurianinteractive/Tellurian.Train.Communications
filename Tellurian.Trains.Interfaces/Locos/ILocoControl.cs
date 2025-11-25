namespace Tellurian.Trains.Interfaces.Locos;

public interface ILocoControl
{
    Task<bool> DriveAsync(LocoAddress address, LocoDrive drive, CancellationToken cancellationToken = default);
    Task<bool> EmergencyStopAsync(LocoAddress address, CancellationToken cancellationToken = default);
    Task<bool> SetFunctionAsync(LocoAddress address, LocoFunction locoFunction, CancellationToken cancellationToken = default);
}
