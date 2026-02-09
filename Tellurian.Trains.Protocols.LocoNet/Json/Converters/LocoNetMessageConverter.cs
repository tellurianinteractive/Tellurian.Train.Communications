using System.Text.Json;
using System.Text.Json.Serialization;
using Tellurian.Trains.Communications.Interfaces.Accessories;
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

        return CreateMessage(typeName, root);
    }

    private static Message CreateMessage(string typeName, JsonElement root)
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
            nameof(GetLocoAddressCommand) => CreateGetLocoAddressCommand(root),
            nameof(SetAccessoryCommand) => CreateSetAccessoryCommand(root),
            nameof(RequestAccessoryStateCommand) => CreateRequestAccessoryStateCommand(root),
            nameof(AccessoryAcknowledgeCommand) => CreateAccessoryAcknowledgeCommand(root),

            // Notifications with raw data
            nameof(SlotNotification) => CreateSlotNotification(root),
            nameof(SensorInputNotification) => CreateSensorInputNotification(root),
            nameof(AccessoryReportNotification) => CreateAccessoryReportNotification(root),
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

    private static GetLocoAddressCommand CreateGetLocoAddressCommand(JsonElement root)
    {
        var address = Tellurian.Trains.Communications.Interfaces.Locos.Address.From(GetInt16Property(root, "address"));
        return new GetLocoAddressCommand(address);
    }

    private static SetAccessoryCommand CreateSetAccessoryCommand(JsonElement root)
    {
        var address = Tellurian.Trains.Communications.Interfaces.Accessories.Address.From(GetInt16Property(root, "address"));
        var direction = GetPositionProperty(root, "direction");
        var output = GetMotorStateProperty(root, "output");
        return new SetAccessoryCommand(address, direction, output);
    }

    private static RequestAccessoryStateCommand CreateRequestAccessoryStateCommand(JsonElement root)
    {
        var address = Tellurian.Trains.Communications.Interfaces.Accessories.Address.From(GetInt16Property(root, "address"));
        return new RequestAccessoryStateCommand(address);
    }

    private static AccessoryAcknowledgeCommand CreateAccessoryAcknowledgeCommand(JsonElement root)
    {
        var address = Tellurian.Trains.Communications.Interfaces.Accessories.Address.From(GetInt16Property(root, "address"));
        var direction = GetPositionProperty(root, "direction");
        var output = GetMotorStateProperty(root, "output");
        return new AccessoryAcknowledgeCommand(address, direction, output);
    }

    private static Position GetPositionProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) ||
            element.TryGetProperty(ToCamelCase(propertyName), out prop))
        {
            var value = prop.GetString();
            return value?.ToLowerInvariant() switch
            {
                "closedorgreen" => Position.ClosedOrGreen,
                "thrownorred" => Position.ThrownOrRed,
                _ => throw new JsonException($"Unknown Position value: {value}")
            };
        }
        throw new JsonException($"Missing required property: {propertyName}");
    }

    private static MotorState GetMotorStateProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) ||
            element.TryGetProperty(ToCamelCase(propertyName), out prop))
        {
            var value = prop.GetString();
            return value?.ToLowerInvariant() switch
            {
                "on" => MotorState.On,
                "off" => MotorState.Off,
                _ => throw new JsonException($"Unknown MotorState value: {value}")
            };
        }
        throw new JsonException($"Missing required property: {propertyName}");
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

    private static AccessoryReportNotification CreateAccessoryReportNotification(JsonElement root)
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

        byte checksum = (byte)(~(AccessoryReportNotification.OperationCode ^ lowBits ^ highBits) & 0xFF);
        return (AccessoryReportNotification)LocoNetMessageFactory.Create([AccessoryReportNotification.OperationCode, lowBits, highBits, checksum]);
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

    public override void Write(Utf8JsonWriter writer, Message value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        // Write the type discriminator
        writer.WriteString("$type", value.GetType().Name);

        // Write common Message property (for backward compatibility)
        writer.WriteString("messageType", value.MessageType);

        // Write type-specific properties explicitly for AOT compatibility
        WriteTypeSpecificProperties(writer, value);

        writer.WriteEndObject();
    }

    private static void WriteTypeSpecificProperties(Utf8JsonWriter writer, Message value)
    {
        switch (value)
        {
            // Simple commands (no additional properties)
            case PowerOnCommand:
            case PowerOffCommand:
            case ForceIdleCommand:
            case MasterBusyNotification:
                break;

            // Commands with simple numeric properties
            case SetLocoSpeedCommand cmd:
                writer.WriteNumber("slot", cmd.Slot);
                writer.WriteNumber("speed", cmd.Speed);
                break;

            case RequestSlotDataCommand cmd:
                writer.WriteNumber("slotNumber", cmd.SlotNumber);
                break;

            case MoveSlotCommand cmd:
                writer.WriteNumber("sourceSlot", cmd.SourceSlot);
                writer.WriteNumber("destinationSlot", cmd.DestinationSlot);
                break;

            case GetLocoAddressCommand cmd:
                writer.WriteNumber("address", cmd.Address.Number);
                break;

            case SetAccessoryCommand cmd:
                writer.WriteNumber("address", cmd.Address.Number);
                writer.WriteString("direction", ToCamelCase(cmd.Direction.ToString()));
                writer.WriteString("output", ToCamelCase(cmd.Output.ToString()));
                break;

            case RequestAccessoryStateCommand cmd:
                writer.WriteNumber("address", cmd.Address.Number);
                break;

            case AccessoryAcknowledgeCommand cmd:
                writer.WriteNumber("address", cmd.Address.Number);
                writer.WriteString("direction", ToCamelCase(cmd.Direction.ToString()));
                writer.WriteString("output", ToCamelCase(cmd.Output.ToString()));
                break;

            // Notifications
            case SlotNotification notification:
                writer.WriteString("rawData", BitConverter.ToString(notification.RawData));
                writer.WriteNumber("slotNumber", notification.SlotNumber);
                writer.WriteNumber("locoAddress", notification.Address);
                writer.WriteNumber("speed", notification.Speed);
                writer.WriteBoolean("direction", notification.Direction);
                writer.WriteString("status", ToCamelCase(notification.Status.ToString()));
                break;

            case SensorInputNotification notification:
                writer.WriteNumber("address", notification.Address);
                writer.WriteBoolean("isSwitchInput", notification.IsSwitchInput);
                writer.WriteBoolean("isHigh", notification.IsHigh);
                break;

            case AccessoryReportNotification notification:
                writer.WriteNumber("address", notification.Address.Number);
                writer.WriteBoolean("isInputFeedback", notification.IsInputFeedback);
                if (notification.IsInputFeedback)
                {
                    writer.WriteBoolean("isSwitchInput", notification.IsSwitchInput);
                    writer.WriteBoolean("isInputHigh", notification.IsInputHigh);
                }
                else
                {
                    writer.WriteBoolean("closedOutputOn", notification.ClosedOutputOn);
                    writer.WriteBoolean("thrownOutputOn", notification.ThrownOutputOn);
                }
                break;

            case LongAcknowledge notification:
                writer.WriteNumber("forOperationCode", notification.ForOperationCode);
                writer.WriteNumber("responseCode", notification.ResponseCode);
                break;

            default:
                // For unknown types, the common properties are sufficient for debugging
                break;
        }
    }

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return char.ToLowerInvariant(value[0]) + value[1..];
    }
}
