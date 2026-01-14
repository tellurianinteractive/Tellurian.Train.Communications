using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Communications.Interfaces.Locos;

[JsonConverter(typeof(DirectionConverter))]
public enum Direction
{
    Forward,
    Backward
}

internal sealed class DirectionConverter() : JsonStringEnumConverter<Direction>(JsonNamingPolicy.CamelCase);