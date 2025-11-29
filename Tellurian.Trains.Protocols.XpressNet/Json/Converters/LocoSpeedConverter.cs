using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Protocols.XpressNet.Json.Converters;

/// <summary>
/// JSON converter for <see cref="LocoSpeed"/> that serializes as an object with code, maxSteps and current.
/// </summary>
public sealed class LocoSpeedConverter : JsonConverter<LocoSpeed>
{
    public override LocoSpeed Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Expected object for LocoSpeed, got {reader.TokenType}");
        }

        byte maxSteps = 126;
        byte current = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected property name");
            }

            var propertyName = reader.GetString();
            reader.Read();

            switch (propertyName?.ToLowerInvariant())
            {
                case "maxsteps":
                    maxSteps = reader.GetByte();
                    break;
                case "current":
                    current = reader.GetByte();
                    break;
            }
        }

        return LocoSpeed.FromNumberOfSteps(maxSteps, current);
    }

    public override void Write(Utf8JsonWriter writer, LocoSpeed value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("maxSteps", value.MaxSteps);
        writer.WriteNumber("current", value.Current);
        writer.WriteEndObject();
    }
}
