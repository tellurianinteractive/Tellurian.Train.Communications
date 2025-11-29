using System.Runtime.Serialization;

namespace Tellurian.Trains.Interfaces.Locos;

[DataContract]
public sealed class LocoFunctionsNotification(Address address, Function[] activeFunctions, DateTimeOffset timestamp) : LocoNotification(address, timestamp)
{
    [DataMember]
    private readonly Function[] _ActiveFunctions = activeFunctions ?? [];

    public LocoFunctionsNotification(Address address, Function[] activeFunctions) : this(address, activeFunctions, DateTimeOffset.Now) { }

    public IEnumerable<Function> ActiveFunctions => _ActiveFunctions;
}
