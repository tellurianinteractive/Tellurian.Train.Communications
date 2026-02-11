using System.Globalization;

namespace Tellurian.Trains.Protocols.LocoNet.Lncv;

/// <summary>
/// Information about a discovered LNCV device on the LocoNet bus.
/// </summary>
public readonly struct LncvDeviceInfo(ushort articleNumber, ushort moduleAddress)
{
    /// <summary>Product code (e.g. 6341 for Uhlenbrock 63410).</summary>
    public ushort ArticleNumber { get; } = articleNumber;

    /// <summary>Module address (stored in LNCV 0).</summary>
    public ushort ModuleAddress { get; } = moduleAddress;

    public override string ToString() => string.Format(CultureInfo.CurrentCulture, "Article {0}, Module {1}", ArticleNumber, ModuleAddress);
}
