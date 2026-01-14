
namespace Tellurian.Trains.Protocols.XpressNet;

/// <summary>
/// The direction of a locomotive.
/// </summary>
public enum LocoDirection
{
    /// <summary>
    /// Locomotive direction shall be backwards.
    /// </summary>
    Backward,
    /// <summary>
    /// Locomotive direction shall be forward.
    /// </summary>
    Forward
}

public static class LocoDirectionExtensions
{
    public static Tellurian.Trains.Communications.Interfaces.Locos.Direction Map(this LocoDirection me) =>
        me == LocoDirection.Forward ? Tellurian.Trains.Communications.Interfaces.Locos.Direction.Forward : Tellurian.Trains.Communications.Interfaces.Locos.Direction.Backward;

    public static LocoDirection Map(this Tellurian.Trains.Communications.Interfaces.Locos.Direction me) =>
        me == Tellurian.Trains.Communications.Interfaces.Locos.Direction.Forward ? LocoDirection.Forward : LocoDirection.Backward;
}
