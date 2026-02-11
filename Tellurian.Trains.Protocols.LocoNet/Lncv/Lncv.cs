using System.Globalization;

namespace Tellurian.Trains.Protocols.LocoNet.Lncv;

/// <summary>
/// Represents an LNCV (LocoNet Configuration Variable) with a 16-bit number and 16-bit value.
/// </summary>
public readonly struct Lncv(ushort number, ushort value)
{
    /// <summary>CV number (0-65535).</summary>
    public ushort Number { get; } = number;

    /// <summary>CV value (0-65535).</summary>
    public ushort Value { get; } = value;

    public override string ToString() => string.Format(CultureInfo.CurrentCulture, "LNCV{0}={1}", Number, Value);
}
