using System.Text.Json;
using System.Text.Json.Serialization;
using Tellurian.Trains.Interfaces.Accessories;
using Tellurian.Trains.Protocols.LocoNet.Commands;
using Tellurian.Trains.Protocols.LocoNet.Notifications;

namespace Tellurian.Trains.Protocols.LocoNet.Json.Converters;

/// <summary>
/// JSON converter for LocoNet <see cref="Message"/> that serializes all public properties
/// and supports deserialization back to the correct message type.
/// </summary>
public sealed class LocoNetMessageConverter : JsonConverter<Message>
{
    public override Message? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Expected StartObject, got {reader.TokenType}");
        }

        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        // Get the type discriminator
        if (!root.TryGetProperty("$type", out var typeProperty))
        {
            throw new JsonException("Missing $type discriminator in LocoNet message JSON");
        }

        var typeName = typeProperty.GetString()
            ?? throw new JsonException("$type discriminator is null");

        return CreateMessage(typeName, root, options);
    }

    private static Message CreateMessage(string typeName, JsonElement root, JsonSerializerOptions options)
    {
        return typeName switch
        {
            // Simple commands (no parameters)
            nameof(PowerOnCommand) => new PowerOnCommand(),
            nameof(PowerOffCommand) => new PowerOffCommand(),
            nameof(ForceIdleCommand) => new ForceIdleCommand(),

            // Commands with parameters
            nameof(SetLocoSpeedCommand) => CreateSetLocoSpeedCommand(root),
            nameof(RequestSlotDataCommand) => CreateRequestSlotDataCommand(root),
            nameof(MoveSlotCommand) => CreateMoveSlotCommand(root),
            nameof(GetLocoAddressCommand) => CreateGetLocoAddressCommand(root, options),
            nameof(SetTurnoutCommand) => CreateSetTurnoutCommand(root, options),
            nameof(RequestSwitchStateCommand) => CreateRequestSwitchStateCommand(root, options),
            nameof(SwitchAcknowledgeCommand) => CreateSwitchAcknowledgeCommand(root, options),

            // Notifications with raw data
            nameof(SlotNotification) => CreateSlotNotification(root),
            nameof(SensorInputNotification) => CreateSensorInputNotification(root),
            nameof(SwitchReportNotification) => CreateSwitchReportNotification(root),
            nameof(MasterBusyNotification) => new MasterBusyNotification(),
            nameof(LongAcknowledge) => CreateLongAcknowledge(root),

            // Unsupported type
            _ => throw new JsonException($"Unknown or unsupported LocoNet message type: {typeName}")
        };
    }

    private static SetLocoSpeedCommand CreateSetLocoSpeedCommand(JsonElement root)
    {
        var slot = GetByteProperty(root, "slot");
        var speed = GetByteProperty(root, "speed");
        return new SetLocoSpeedCommand(slot, speed);
    }

    private static RequestSlotDataCommand CreateRequestSlotDataCommand(JsonElement root)
    {
        var slotNumber = GetByteProperty(root, "slotNumber");
        return new RequestSlotDataCommand(slotNumber);
    }

    private static MoveSlotCommand CreateMoveSlotCommand(JsonElement root)
    {
        var sourceSlot = GetByteProperty(root, "sourceSlot");
        var destinationSlot = GetByteProperty(root, "destinationSlot");
        return new MoveSlotCommand(sourceSlot, destinationSlot);
    }

    private static GetLocoAddressCommand CreateGetLocoAddressCommand(JsonElement root, JsonSerializerOptions options)
    {
        var address = GetProperty<Interfaces.Locos.Address>(root, "address", options);
        return new GetLocoAddressCommand(address);
    }

    private static SetTurnoutCommand CreateSetTurnoutCommand(JsonElement root, JsonSerializerOptions options)
    {
        var address = GetProperty<Interfaces.Accessories.Address>(root, "address", options);
        var direction = GetProperty<Position>(root, "direction", options);
        var output = GetProperty<MotorState>(root, "output", options);
        return new SetTurnoutCommand(address, direction, output);
    }

    private static RequestSwitchStateCommand CreateRequestSwitchStateCommand(JsonElement root, JsonSerializerOptions options)
    {
        var address = GetProperty<Interfaces.Accessories.Address>(root, "address", options);
        return new RequestSwitchStateCommand(address);
    }

    private static SwitchAcknowledgeCommand CreateSwitchAcknowledgeCommand(JsonElement root, JsonSerializerOptions options)
    {
        var address = GetProperty<Interfaces.Accessories.Address>(root, "address", options);
        var direction = GetProperty<Position>(root, "direction", options);
        var output = GetProperty<MotorState>(root, "output", options);
        return new SwitchAcknowledgeCommand(address, direction, output);
    }

    private static SlotNotification CreateSlotNotification(JsonElement root)
    {
        var rawData = GetBytesFromHex(root, "rawData");
        return (SlotNotification)LocoNetMessageFactory.Create(rawData);
    }

    private static SensorInputNotification CreateSensorInputNotification(JsonElement root)
    {
        // Reconstruct binary data from properties
        var address = GetUInt16Property(root, "address");
        var isSwitchInput = GetBoolProperty(root, "isSwitchInput");
        var isHigh = GetBoolProperty(root, "isHigh");

        // Encode back to binary format
        byte in1 = (byte)(address & 0x7F);
        byte in2 = (byte)(((address >> 7) & 0x0F) | 0x40 | (isSwitchInput ? 0x20 : 0) | (isHigh ? 0x10 : 0));
        byte checksum = (byte)(~(SensorInputNotification.OperationCode ^ in1 ^ in2) & 0xFF);

        return (SensorInputNotification)LocoNetMessageFactory.Create([SensorInputNotification.OperationCode, in1, in2, checksum]);
    }

    private static SwitchReportNotification CreateSwitchReportNotification(JsonElement root)
    {
        var addressNumber = GetInt16Property(root, "address");
        var isInputFeedback = GetBoolProperty(root, "isInputFeedback");

        byte lowBits = (byte)(addressNumber & 0x7F);
        byte highBits = (byte)((addressNumber >> 7) & 0x0F);

        if (isInputFeedback)
        {
            var isSwitchInput = GetBoolProperty(root, "isSwitchInput");
            var isInputHigh = GetBoolProperty(root, "isInputHigh");
            highBits |= 0x40; // Set bit 6 for input feedback
            if (isSwitchInput) highBits |= 0x20;
            if (isInputHigh) highBits |= 0x10;
        }
        else
        {
            var closedOutputOn = GetBoolProperty(root, "closedOutputOn");
            var thrownOutputOn = GetBoolProperty(root, "thrownOutputOn");
            if (closedOutputOn) highBits |= 0x20;
            if (thrownOutputOn) highBits |= 0x10;
        }

        byte checksum = (byte)(~(SwitchReportNotification.OperationCode ^ lowBits ^ highBits) & 0xFF);
        return (SwitchReportNotification)LocoNetMessageFactory.Create([SwitchReportNotification.OperationCode, lowBits, highBits, checksum]);
    }

    private static LongAcknowledge CreateLongAcknowledge(JsonElement root)
    {
        // LongAcknowledge stores forOperationCode with MSB set internally, but receives without it in the message
        var forOpCode = GetByteProperty(root, "forOperationCode");
        var responseCode = GetByteProperty(root, "responseCode");
        // The data byte is forOperationCode with MSB cleared
        byte opCodeByte = (byte)(forOpCode & 0x7F);
        byte checksum = (byte)(~(LongAcknowledge.OperationCode ^ opCodeByte ^ responseCode) & 0xFF);
        return (LongAcknowledge)LocoNetMessageFactory.Create([LongAcknowledge.OperationCode, opCodeByte, responseCode, checksum]);
    }

    private static byte GetByteProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) ||
            element.TryGetProperty(ToCamelCase(propertyName), out prop))
        {
            return prop.GetByte();
        }
        throw new JsonException($"Missing required property: {propertyName}");
    }

    private static ushort GetUInt16Property(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) ||
            element.TryGetProperty(ToCamelCase(propertyName), out prop))
        {
            return prop.GetUInt16();
        }
        throw new JsonException($"Missing required property: {propertyName}");
    }

    private static short GetInt16Property(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) ||
            element.TryGetProperty(ToCamelCase(propertyName), out prop))
        {
            return prop.GetInt16();
        }
        throw new JsonException($"Missing required property: {propertyName}");
    }

    private static bool GetBoolProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) ||
            element.TryGetProperty(ToCamelCase(propertyName), out prop))
        {
            return prop.GetBoolean();
        }
        throw new JsonException($"Missing required property: {propertyName}");
    }

    private static T GetProperty<T>(JsonElement element, string propertyName, JsonSerializerOptions options)
    {
        if (element.TryGetProperty(propertyName, out var prop) ||
            element.TryGetProperty(ToCamelCase(propertyName), out prop))
        {
            return JsonSerializer.Deserialize<T>(prop.GetRawText(), options)
                ?? throw new JsonException($"Failed to deserialize property: {propertyName}");
        }
        throw new JsonException($"Missing required property: {propertyName}");
    }

    private static byte[] GetBytesFromHex(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) ||
            element.TryGetProperty(ToCamelCase(propertyName), out prop))
        {
            var hex = prop.GetString() ?? throw new JsonException($"Property {propertyName} is null");
            return hex.Split('-').Select(h => Convert.ToByte(h, 16)).ToArray();
        }
        throw new JsonException($"Missing required property: {propertyName}");
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return char.ToLowerInvariant(name[0]) + name[1..];
    }

    public override void Write(Utf8JsonWriter writer, Message value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        // Write the type discriminator
        writer.WriteString("$type", value.GetType().Name);

        // Write type-specific properties using reflection
        WriteTypeSpecificProperties(writer, value, options);

        writer.WriteEndObject();
    }

    private static void WriteTypeSpecificProperties(Utf8JsonWriter writer, Message value, JsonSerializerOptions options)
    {
        var type = value.GetType();

        foreach (var property in type.GetProperties())
        {
            // Skip properties with JsonIgnore attribute
            if (property.GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Length > 0)
                continue;

            // Skip indexers
            if (property.GetIndexParameters().Length > 0)
                continue;

            try
            {
                var propertyValue = property.GetValue(value);
                var propertyName = GetPropertyName(property.Name, options);

                if (propertyValue == null)
                {
                    if (options.DefaultIgnoreCondition != JsonIgnoreCondition.WhenWritingNull)
                    {
                        writer.WriteNull(propertyName);
                    }
                }
                else if (propertyValue is byte[] byteArray)
                {
                    // Serialize byte arrays as hex strings
                    writer.WriteString(propertyName, BitConverter.ToString(byteArray));
                }
                else
                {
                    writer.WritePropertyName(propertyName);
                    JsonSerializer.Serialize(writer, propertyValue, propertyValue.GetType(), options);
                }
            }
            catch
            {
                // Skip properties that throw exceptions when accessed
            }
        }
    }

    private static string GetPropertyName(string propertyName, JsonSerializerOptions options)
    {
        if (options.PropertyNamingPolicy != null)
        {
            return options.PropertyNamingPolicy.ConvertName(propertyName);
        }
        return propertyName;
    }
}
