using System.Runtime.Serialization;

namespace Tellurian.Trains.Interfaces.Locos;

[DataContract]
public sealed class FunctionsLocoNotification(LocoAddress address, LocoFunction[] activeFunctions, DateTimeOffset timestamp) : LocoNotification(address, timestamp)
{
    [DataMember]
    private readonly LocoFunction[] _ActiveFunctions = activeFunctions ?? [];

    public FunctionsLocoNotification(LocoAddress address, LocoFunction[] activeFunctions) : this(address, activeFunctions, DateTimeOffset.Now) { }

    public IEnumerable<LocoFunction> ActiveFunctions => _ActiveFunctions;
}
