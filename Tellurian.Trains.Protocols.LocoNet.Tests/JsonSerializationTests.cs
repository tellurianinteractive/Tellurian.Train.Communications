using System.Text.Json;
using Tellurian.Trains.Communications.Interfaces.Accessories;
using Tellurian.Trains.Protocols.LocoNet.Commands;
using Tellurian.Trains.Protocols.LocoNet.Json;
using LocoAddress = Tellurian.Trains.Communications.Interfaces.Locos.Address;
using AccessoryAddress = Tellurian.Trains.Communications.Interfaces.Accessories.Address;

namespace Tellurian.Trains.Protocols.LocoNet.Tests;

[TestClass]
public class JsonSerializationTests
{
    [TestMethod]
    public void PowerOnCommand_SerializesToJson()
    {
        var command = new PowerOnCommand();

        var json = JsonSerializer.Serialize<Message>(command, LocoNetJsonSerializationOptions.Default);

        Assert.Contains("\"$type\":\"PowerOnCommand\"", json);
        Assert.Contains("\"messageType\":\"PowerOnCommand\"", json);
    }

    [TestMethod]
    public void PowerOffCommand_SerializesToJson()
    {
        var command = new PowerOffCommand();

        var json = JsonSerializer.Serialize<Message>(command, LocoNetJsonSerializationOptions.Default);

        Assert.Contains("\"$type\":\"PowerOffCommand\"", json);
    }

    [TestMethod]
    public void ForceIdleCommand_SerializesToJson()
    {
        var command = new ForceIdleCommand();

        var json = JsonSerializer.Serialize<Message>(command, LocoNetJsonSerializationOptions.Default);

        Assert.Contains("\"$type\":\"ForceIdleCommand\"", json);
    }

    [TestMethod]
    public void CreateIndented_ReturnsFormattedJson()
    {
        var options = LocoNetJsonSerializationOptions.CreateIndented();
        var command = new PowerOnCommand();

        var json = JsonSerializer.Serialize<Message>(command, options);

        // Indented JSON should contain newlines
        Assert.Contains("\n", json);
    }

    // Round-trip tests

    [TestMethod]
    public void PowerOnCommand_RoundTrip()
    {
        var original = new PowerOnCommand();

        var json = JsonSerializer.Serialize<Message>(original, LocoNetJsonSerializationOptions.Default);
        var deserialized = JsonSerializer.Deserialize<Message>(json, LocoNetJsonSerializationOptions.Default);

        Assert.IsInstanceOfType<PowerOnCommand>(deserialized);
    }

    [TestMethod]
    public void PowerOffCommand_RoundTrip()
    {
        var original = new PowerOffCommand();

        var json = JsonSerializer.Serialize<Message>(original, LocoNetJsonSerializationOptions.Default);
        var deserialized = JsonSerializer.Deserialize<Message>(json, LocoNetJsonSerializationOptions.Default);

        Assert.IsInstanceOfType<PowerOffCommand>(deserialized);
    }

    [TestMethod]
    public void ForceIdleCommand_RoundTrip()
    {
        var original = new ForceIdleCommand();

        var json = JsonSerializer.Serialize<Message>(original, LocoNetJsonSerializationOptions.Default);
        var deserialized = JsonSerializer.Deserialize<Message>(json, LocoNetJsonSerializationOptions.Default);

        Assert.IsInstanceOfType<ForceIdleCommand>(deserialized);
    }

    [TestMethod]
    public void SetLocoSpeedCommand_RoundTrip()
    {
        var original = new SetLocoSpeedCommand(5, 75);

        var json = JsonSerializer.Serialize<Message>(original, LocoNetJsonSerializationOptions.Default);
        var deserialized = JsonSerializer.Deserialize<Message>(json, LocoNetJsonSerializationOptions.Default);

        Assert.IsInstanceOfType<SetLocoSpeedCommand>(deserialized);
        var result = (SetLocoSpeedCommand)deserialized!;
        Assert.AreEqual(5, result.Slot);
        Assert.AreEqual(75, result.Speed);
    }

    [TestMethod]
    public void RequestSlotDataCommand_RoundTrip()
    {
        var original = new RequestSlotDataCommand(10);

        var json = JsonSerializer.Serialize<Message>(original, LocoNetJsonSerializationOptions.Default);
        var deserialized = JsonSerializer.Deserialize<Message>(json, LocoNetJsonSerializationOptions.Default);

        Assert.IsInstanceOfType<RequestSlotDataCommand>(deserialized);
        var result = (RequestSlotDataCommand)deserialized!;
        Assert.AreEqual(10, result.SlotNumber);
    }

    [TestMethod]
    public void MoveSlotCommand_RoundTrip()
    {
        var original = new MoveSlotCommand(3, 5);

        var json = JsonSerializer.Serialize<Message>(original, LocoNetJsonSerializationOptions.Default);
        var deserialized = JsonSerializer.Deserialize<Message>(json, LocoNetJsonSerializationOptions.Default);

        Assert.IsInstanceOfType<MoveSlotCommand>(deserialized);
        var result = (MoveSlotCommand)deserialized!;
        Assert.AreEqual(3, result.SourceSlot);
        Assert.AreEqual(5, result.DestinationSlot);
    }

    [TestMethod]
    public void GetLocoAddressCommand_RoundTrip()
    {
        var original = new GetLocoAddressCommand(LocoAddress.From(1234));

        var json = JsonSerializer.Serialize<Message>(original, LocoNetJsonSerializationOptions.Default);
        var deserialized = JsonSerializer.Deserialize<Message>(json, LocoNetJsonSerializationOptions.Default);

        Assert.IsInstanceOfType<GetLocoAddressCommand>(deserialized);
        var result = (GetLocoAddressCommand)deserialized!;
        Assert.AreEqual(1234, result.Address.Number);
    }

    [TestMethod]
    public void SetAccessoryCommand_RoundTrip()
    {
        var original = new SetAccessoryCommand(
            AccessoryAddress.From(100),
            Position.ThrownOrRed,
            MotorState.On);

        var json = JsonSerializer.Serialize<Message>(original, LocoNetJsonSerializationOptions.Default);
        var deserialized = JsonSerializer.Deserialize<Message>(json, LocoNetJsonSerializationOptions.Default);

        Assert.IsInstanceOfType<SetAccessoryCommand>(deserialized);
        var result = (SetAccessoryCommand)deserialized!;
        Assert.AreEqual(100, result.Address.Number);
        Assert.AreEqual(Position.ThrownOrRed, result.Direction);
        Assert.AreEqual(MotorState.On, result.Output);
    }

    [TestMethod]
    public void RequestAccessoryStateCommand_RoundTrip()
    {
        var original = new RequestAccessoryStateCommand(AccessoryAddress.From(50));

        var json = JsonSerializer.Serialize<Message>(original, LocoNetJsonSerializationOptions.Default);
        var deserialized = JsonSerializer.Deserialize<Message>(json, LocoNetJsonSerializationOptions.Default);

        Assert.IsInstanceOfType<RequestAccessoryStateCommand>(deserialized);
        var result = (RequestAccessoryStateCommand)deserialized!;
        Assert.AreEqual(50, result.Address.Number);
    }

    [TestMethod]
    public void AccessoryAcknowledgeCommand_RoundTrip()
    {
        var original = new AccessoryAcknowledgeCommand(
            AccessoryAddress.From(200),
            Position.ClosedOrGreen,
            MotorState.Off);

        var json = JsonSerializer.Serialize<Message>(original, LocoNetJsonSerializationOptions.Default);
        var deserialized = JsonSerializer.Deserialize<Message>(json, LocoNetJsonSerializationOptions.Default);

        Assert.IsInstanceOfType<AccessoryAcknowledgeCommand>(deserialized);
        var result = (AccessoryAcknowledgeCommand)deserialized!;
        Assert.AreEqual(200, result.Address.Number);
        Assert.AreEqual(Position.ClosedOrGreen, result.Direction);
        Assert.AreEqual(MotorState.Off, result.Output);
    }
}
