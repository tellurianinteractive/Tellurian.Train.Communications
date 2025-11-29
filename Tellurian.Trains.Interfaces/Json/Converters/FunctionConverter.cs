using System.Text.Json;
using System.Text.Json.Serialization;
using Tellurian.Trains.Interfaces.Locos;

namespace Tellurian.Trains.Interfaces.Json.Converters;

/// <summary>
/// JSON converter for <see cref="Function"/> that serializes as an object with number and isOn.
/// </summary>
public sealed class FunctionConverter : JsonConverter<Function>
{
    public override Function Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Expected object for Function, got {reader.TokenType}");
        }

        Functions number = Functions.F0;
        bool isOn = false;

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
                case "number":
                    if (reader.TokenType == JsonTokenType.String)
                    {
                        var enumValue = reader.GetString();
                        if (Enum.TryParse<Functions>(enumValue, ignoreCase: true, out var parsed))
                        {
                            number = parsed;
                        }
                    }
                    else if (reader.TokenType == JsonTokenType.Number)
                    {
                        number = (Functions)reader.GetByte();
                    }
                    break;
                case "ison":
                    isOn = reader.GetBoolean();
                    break;
            }
        }

        return Function.Set(number, isOn);
    }

    public override void Write(Utf8JsonWriter writer, Function value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("number", value.Number.ToString());
        writer.WriteBoolean("isOn", value.IsOn);
        writer.WriteEndObject();
    }
}
