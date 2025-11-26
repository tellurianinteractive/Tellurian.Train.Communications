namespace Tellurian.Trains.Protocols.XpressNet.Commands;

/// <summary>
/// Set command station power-up mode command (spec section 4.16).
/// Configures how the command station behaves when powered on.
/// </summary>
/// <remarks>
/// Format: Header=0x22, Data=[0x22, M]
/// - M=0: Manual Start Mode - no speed/direction commands sent on power up
/// - M=1: Automatic Start Mode - locomotives resume last known speed/direction/functions
///
/// Not all command stations support this request.
///
/// Response: None
/// </remarks>
public sealed class SetPowerUpModeCommand : Command
{
    /// <summary>
    /// Creates a command to set the power-up mode.
    /// </summary>
    /// <param name="mode">The power-up mode to set</param>
    public SetPowerUpModeCommand(PowerUpMode mode)
        : base(0x22, [0x22, (byte)mode]) { }

    /// <summary>
    /// Creates a command to set manual power-up mode.
    /// Locomotives will have speed 0 and functions off on power up.
    /// </summary>
    public static SetPowerUpModeCommand Manual => new(PowerUpMode.Manual);

    /// <summary>
    /// Creates a command to set automatic power-up mode.
    /// Locomotives will resume their last known state on power up.
    /// </summary>
    public static SetPowerUpModeCommand Automatic => new(PowerUpMode.Automatic);
}

/// <summary>
/// Command station power-up modes.
/// </summary>
public enum PowerUpMode : byte
{
    /// <summary>
    /// Manual Start Mode - no speed and direction commands are sent to
    /// locomotives on power up. All locomotives have speed 0 and functions off.
    /// </summary>
    Manual = 0,

    /// <summary>
    /// Automatic Start Mode - on power up the command station sends DCC packets
    /// to all known locomotives with the last known speed, direction, and function status.
    /// </summary>
    Automatic = 1
}
