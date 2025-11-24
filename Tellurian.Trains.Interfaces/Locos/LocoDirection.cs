using System.Runtime.Serialization;

namespace Tellurian.Trains.Interfaces.Locos;

[DataContract]
public enum LocoDirection
{
    Forward,
    Backward
}