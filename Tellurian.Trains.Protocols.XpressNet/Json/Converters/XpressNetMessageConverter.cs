using System.Text.Json;
using System.Text.Json.Serialization;
using Tellurian.Trains.Protocols.XpressNet.Notifications;

namespace Tellurian.Trains.Protocols.XpressNet.Json.Converters;

/// <summary>
/// JSON converter for XpressNet <see cref="Message"/> that serializes message properties.
/// Supports polymorphic serialization with type discrimination for logging/debugging purposes.
/// AOT-compatible: does not use reflection-based serialization.
/// </summary>
public sealed class XpressNetMessageConverter : JsonConverter<Message>
{
    public override Message? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // XpressNet messages are typically created from binary data, not JSON.
        // This converter supports serialization for logging/debugging purposes.
        // Deserialization would require additional context (like the binary buffer).
        throw new NotSupportedException(
            "Deserialization of XpressNet messages from JSON is not supported. " +
            "Use NotificationFactory.Create() or command constructors with binary data.");
    }

    public override void Write(Utf8JsonWriter writer, Message value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        // Write the type discriminator
        writer.WriteString("$type", value.GetType().Name);

        // Write common properties
        writer.WriteNumber("header", value.Header);
        writer.WriteNumber("length", value.Length);
        writer.WriteString("dataHex", value.DataHex);

        // Write type-specific properties explicitly for AOT compatibility
        WriteTypeSpecificProperties(writer, value);

        writer.WriteEndObject();
    }

    private static void WriteTypeSpecificProperties(Utf8JsonWriter writer, Message value)
    {
        switch (value)
        {
            // Notifications with meaningful properties
            case LocoInfoNotification notification:
                writer.WriteNumber("address", notification.Address.Number);
                writer.WriteString("direction", ToCamelCase(notification.Direction.ToString()));
                writer.WriteBoolean("isControlledByOtherDevice", notification.IsControlledByOtherDevice);
                writer.WriteNumber("speedMaxSteps", notification.Speed.MaxSteps);
                writer.WriteNumber("speedCurrent", notification.Speed.Current);
                break;

            case VersionNotification notification:
                writer.WriteString("version", notification.Version);
                writer.WriteString("busName", notification.BusName);
                writer.WriteString("commandStationName", notification.CommandStationName);
                break;

            case FirmwareNotification notification:
                writer.WriteNumber("majorVersion", notification.MajorVersion);
                writer.WriteNumber("minorVersion", notification.MinorVersion);
                break;

            case AccessoryDecoderInfoNotification notification:
                writer.WriteNumber("groupAddress", notification.GroupAddress);
                writer.WriteNumber("firstTurnoutAddress", notification.FirstTurnoutAddress);
                writer.WriteNumber("secondTurnoutAddress", notification.SecondTurnoutAddress);
                break;

            case LocoOperatedByAnotherDeviceNotification notification:
                writer.WriteNumber("locoAddress", notification.LocoAddress.Number);
                break;

            case AddressRetrievalNotification notification:
                writer.WriteNumber("locoAddress", notification.LocoAddress.Number);
                break;

            // Simple notifications and broadcasts (no additional properties beyond common ones)
            case TrackPowerOnBroadcast:
            case TrackPowerOffBroadcast:
            case EmergencyStopBroadcast:
            case TrackShortCircuitNotification:
            case TransferErrorNotification:
            case CommandStationBusyNotification:
            case UnknownCommandNotification:
                // No additional properties to write
                break;

            default:
                // For unknown types, the common properties (header, length, dataHex) are sufficient for debugging
                break;
        }
    }

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return char.ToLowerInvariant(value[0]) + value[1..];
    }
}
