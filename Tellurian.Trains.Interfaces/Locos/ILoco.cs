namespace Tellurian.Trains.Interfaces.Locos;

/// <summary>
/// Defines an interface for controlling a locomotive, including driving, emergency stopping, and setting functions.
/// </summary>
/// <remarks>Implementations of this interface provide asynchronous methods for interacting with locomotives,
/// typically in a model railway control system. All operations are performed asynchronously and may be cancelled using
/// the provided cancellation token. Thread safety and specific behavior depend on the concrete
/// implementation.</remarks>
public interface ILoco
{
    Task<bool> DriveAsync(Address address, Drive drive, CancellationToken cancellationToken = default);
    Task<bool> EmergencyStopAsync(Address address, CancellationToken cancellationToken = default);
    Task<bool> SetFunctionAsync(Address address, Function locoFunction, CancellationToken cancellationToken = default);
}
