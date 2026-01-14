using System.Text.Json;
using System.Text.Json.Serialization;
using Tellurian.Trains.Communications.Interfaces.Locos;

namespace Tellurian.Trains.Communications.Interfaces.Json.Converters;

/// <summary>
/// JSON converter for <see cref="Speed"/> that serializes as an object with maxSteps and currentStep.
/// </summary>
public sealed class SpeedConverter : JsonConverter<Speed>
{
    public override Speed Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Expected object for Speed, got {reader.TokenType}");
        }

        byte maxSteps = 126;
        byte currentStep = 0;

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
                case "currentstep":
                    currentStep = reader.GetByte();
                    break;
            }
        }

        return Speed.Set(ToLocoSpeedSteps(maxSteps), currentStep);
    }

    private static LocoSpeedSteps ToLocoSpeedSteps(byte value) =>
        value switch
        {
            14 => LocoSpeedSteps.Steps14,
            27 => LocoSpeedSteps.Steps27,
            28 => LocoSpeedSteps.Steps28,
            _ => LocoSpeedSteps.Steps126
        };

    public override void Write(Utf8JsonWriter writer, Speed value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("maxSteps", (byte)value.MaxSteps);
        writer.WriteNumber("currentStep", value.CurrentStep);
        writer.WriteEndObject();
    }
}

