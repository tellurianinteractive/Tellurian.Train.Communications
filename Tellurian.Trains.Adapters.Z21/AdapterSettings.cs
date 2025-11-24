using System.Net;

namespace Tellurian.Trains.Adapters.Z21;

public class AdapterSettings
{
    public static AdapterSettings Default => new AdapterSettings();
    public IPAddress Address { get; set; } = IPAddress.Parse("192.168.0.111");
    public int CommandPort { get; set; } = 21005;
    public int NotificationPort { get; set; } = 21006;
}
