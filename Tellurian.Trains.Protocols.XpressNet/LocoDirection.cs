
namespace Tellurian.Trains.Protocols.XpressNet; 
/// <summary>
/// The direction of a locomotive.
/// </summary>
public enum LocoDirection {
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
    public static Interfaces.Locos.LocoDirection Map(this LocoDirection me) =>
        me == LocoDirection.Forward ? Interfaces.Locos.LocoDirection.Forward : Interfaces.Locos.LocoDirection.Backward;

    public static LocoDirection Map(this Interfaces.Locos.LocoDirection me) =>
        me == Interfaces.Locos.LocoDirection.Forward ? LocoDirection.Forward : LocoDirection.Backward;
}
