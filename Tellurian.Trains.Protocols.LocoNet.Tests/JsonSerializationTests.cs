using System.Text.Json;
using Tellurian.Trains.Interfaces.Accessories;
using Tellurian.Trains.Protocols.LocoNet.Commands;
using Tellurian.Trains.Protocols.LocoNet.Json;
using LocoAddress = Tellurian.Trains.Interfaces.Locos.Address;
using AccessoryAddress = Tellurian.Trains.Interfaces.Accessories.Address;

namespace Tellurian.Trains.Protocols.LocoNet.Tests;

[TestClass]
public class JsonSerializationTests
{
    [TestMethod]
    public void PowerOnCommand_SerializesToJson()
    {
        var command = new PowerOnCommand();

        var json = JsonSerializer.Serialize<Message>(command, LocoNetJsonSerializationOptions.Default);

        Assert.IsTrue(json.Contains("\"$type\":\"PowerOnCommand\""));
        Assert.IsTrue(json.Contains("\"messageType\":\"PowerOnCommand\""));
    }

    [TestMethod]
    public void PowerOffCommand_SerializesToJson()
    {
        var command = new PowerOffCommand();

        var json = JsonSerializer.Serialize<Message>(command, LocoNetJsonSerializationOptions.Default);

        Assert.IsTrue(json.Contains("\"$type\":\"PowerOffCommand\""));
    }

    [TestMethod]
    public void ForceIdleCommand_SerializesToJson()
    {
        var command = new ForceIdleCommand();

        var json = JsonSerializer.Serialize<Message>(command, LocoNetJsonSerializationOptions.Default);

        Assert.IsTrue(json.Contains("\"$type\":\"ForceIdleCommand\""));
    }

    [TestMethod]
    public void CreateIndented_ReturnsFormattedJson()
    {
        var options = LocoNetJsonSerializationOptions.CreateIndented();
        var command = new PowerOnCommand();

        var json = JsonSerializer.Serialize<Message>(command, options);

        // Indented JSON should contain newlines
        Assert.IsTrue(json.Contains("\n") || json.Contains("\r\n"));
    }

    // Round-trip tests

    [TestMethod]
    public void PowerOnCommand_RoundTrip()
    {
        var original = new PowerOnCommand();

        var json = JsonSerializer.Serialize<Message>(original, LocoNetJsonSerializationOptions.Default);
        var deserialized = JsonSerializer.Deserialize<Message>(json, LocoNetJsonSerializationOptions.Default);

        Assert.IsInstanceOfType(deserialized, typeof(PowerOnCommand));
    }

    [TestMethod]
    public void PowerOffCommand_RoundTrip()
    {
        var original = new PowerOffCommand();

        var json = JsonSerializer.Serialize<Message>(original, LocoNetJsonSerializationOptions.Default);
        var deserialized = JsonSerializer.Deserialize<Message>(json, LocoNetJsonSerializationOptions.Default);

        Assert.IsInstanceOfType(deserialized, typeof(PowerOffCommand));
    }

    [TestMethod]
    public void ForceIdleCommand_RoundTrip()
    {
        var original = new ForceIdleCommand();

        var json = JsonSerializer.Serialize<Message>(original, LocoNetJsonSerializationOptions.Default);
        var deserialized = JsonSerializer.Deserialize<Message>(json, LocoNetJsonSerializationOptions.Default);

        Assert.IsInstanceOfType(deserialized, typeof(ForceIdleCommand));
    }

    [TestMethod]
    public void SetLocoSpeedCommand_RoundTrip()
    {
        var original = new SetLocoSpeedCommand(5, 75);

        var json = JsonSerializer.Serialize<Message>(original, LocoNetJsonSerializationOptions.Default);
        var deserialized = JsonSerializer.Deserialize<Message>(json, LocoNetJsonSerializationOptions.Default);

        Assert.IsInstanceOfType(deserialized, typeof(SetLocoSpeedCommand));
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

        Assert.IsInstanceOfType(deserialized, typeof(RequestSlotDataCommand));
        var result = (RequestSlotDataCommand)deserialized!;
        Assert.AreEqual(10, result.SlotNumber);
    }

    [TestMethod]
    public void MoveSlotCommand_RoundTrip()
    {
        var original = new MoveSlotCommand(3, 5);

        var json = JsonSerializer.Serialize<Message>(original, LocoNetJsonSerializationOptions.Default);
        var deserialized = JsonSerializer.Deserialize<Message>(json, LocoNetJsonSerializationOptions.Default);

        Assert.IsInstanceOfType(deserialized, typeof(MoveSlotCommand));
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

        Assert.IsInstanceOfType(deserialized, typeof(GetLocoAddressCommand));
        var result = (GetLocoAddressCommand)deserialized!;
        Assert.AreEqual(1234, result.Address.Number);
    }

    [TestMethod]
    public void SetTurnoutCommand_RoundTrip()
    {
        var original = new SetTurnoutCommand(
            AccessoryAddress.From(100),
            Position.ThrownOrRed,
            MotorState.On);

        var json = JsonSerializer.Serialize<Message>(original, LocoNetJsonSerializationOptions.Default);
        var deserialized = JsonSerializer.Deserialize<Message>(json, LocoNetJsonSerializationOptions.Default);

        Assert.IsInstanceOfType(deserialized, typeof(SetTurnoutCommand));
        var result = (SetTurnoutCommand)deserialized!;
        Assert.AreEqual(100, result.Address.Number);
        Assert.AreEqual(Position.ThrownOrRed, result.Direction);
        Assert.AreEqual(MotorState.On, result.Output);
    }

    [TestMethod]
    public void RequestSwitchStateCommand_RoundTrip()
    {
        var original = new RequestSwitchStateCommand(AccessoryAddress.From(50));

        var json = JsonSerializer.Serialize<Message>(original, LocoNetJsonSerializationOptions.Default);
        var deserialized = JsonSerializer.Deserialize<Message>(json, LocoNetJsonSerializationOptions.Default);

        Assert.IsInstanceOfType(deserialized, typeof(RequestSwitchStateCommand));
        var result = (RequestSwitchStateCommand)deserialized!;
        Assert.AreEqual(50, result.Address.Number);
    }

    [TestMethod]
    public void SwitchAcknowledgeCommand_RoundTrip()
    {
        var original = new SwitchAcknowledgeCommand(
            AccessoryAddress.From(200),
            Position.ClosedOrGreen,
            MotorState.Off);

        var json = JsonSerializer.Serialize<Message>(original, LocoNetJsonSerializationOptions.Default);
        var deserialized = JsonSerializer.Deserialize<Message>(json, LocoNetJsonSerializationOptions.Default);

        Assert.IsInstanceOfType(deserialized, typeof(SwitchAcknowledgeCommand));
        var result = (SwitchAcknowledgeCommand)deserialized!;
        Assert.AreEqual(200, result.Address.Number);
        Assert.AreEqual(Position.ClosedOrGreen, result.Direction);
        Assert.AreEqual(MotorState.Off, result.Output);
    }
}
