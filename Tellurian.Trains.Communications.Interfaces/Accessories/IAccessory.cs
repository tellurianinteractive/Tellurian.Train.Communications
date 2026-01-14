namespace Tellurian.Trains.Communications.Interfaces.Accessories;

/// <summary>
/// Protocol-agnostic interface for controlling accessories (switches/turnouts/signals).
/// </summary>
public interface IAccessory
{
    /// <summary>
    /// Sets the state of an accessory.
    /// </summary>
    /// <param name="address">The accessory address.</param>
    /// <param name="command">The command specifying function and output state.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>True if the command was sent successfully; otherwise false.</returns>
    Task<bool> SetAccessoryAsync(Address address, AccessoryCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries the current state of an accessory.
    /// </summary>
    /// <param name="address">The accessory address.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>True if the query was sent successfully; otherwise false.</returns>
    Task<bool> QueryAccessoryStateAsync(Address address, CancellationToken cancellationToken = default);
}
