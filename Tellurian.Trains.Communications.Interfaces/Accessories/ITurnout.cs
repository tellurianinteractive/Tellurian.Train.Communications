namespace Tellurian.Trains.Communications.Interfaces.Accessories;

/// <summary>
/// Defines an interface for controlling turnout accessories, providing asynchronous methods to change their state.
/// </summary>
/// <remarks>Implementations of this interface allow clients to operate turnout devices by setting them to
/// 'thrown', 'closed', or 'off' states. All operations are asynchronous and support cancellation via a cancellation
/// token. The meaning of 'thrown' and 'closed' may vary depending on the specific accessory; refer to the
/// implementation or device documentation for details.</remarks>
public interface ITurnout
{
    Task<bool> SetThrownAsync(Address address, bool turnOn = true, CancellationToken cancellationToken = default);
    Task<bool> SetClosedAsync(Address address, bool activate = true, CancellationToken cancellationToken = default);
    Task<bool> TurnOffAsync(Address address, CancellationToken cancellationToken = default);

}
