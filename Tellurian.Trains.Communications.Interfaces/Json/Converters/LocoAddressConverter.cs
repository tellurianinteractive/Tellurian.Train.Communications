using System.Text.Json;
using System.Text.Json.Serialization;
using Tellurian.Trains.Communications.Interfaces.Locos;

namespace Tellurian.Trains.Communications.Interfaces.Json.Converters;

/// <summary>
/// JSON converter for <see cref="Locos.Address"/> that serializes as a simple number.
/// </summary>
public sealed class LocoAddressConverter : JsonConverter<Address>
{
    public override Address Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            return Address.From(reader.GetInt16());
        }
        throw new JsonException($"Expected number for Address, got {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, Address value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Number);
    }
}
