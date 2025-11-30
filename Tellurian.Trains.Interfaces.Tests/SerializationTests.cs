using System.Text.Json;
using Tellurian.Trains.Interfaces.Accessories;
using Tellurian.Trains.Interfaces.Json;
using Tellurian.Trains.Interfaces.Locos;
using LocoAddress = Tellurian.Trains.Interfaces.Locos.Address;
using AccessoryAddress = Tellurian.Trains.Interfaces.Accessories.Address;

namespace Tellurian.Trains.Interfaces.Tests;

[TestClass]
public class SerializationTests
{
    [TestMethod]
    public void LocoDrive_SerializesAndDeserializes()
    {
        var target = new Drive()
        {
            Direction = Direction.Forward,
            Speed = Speed.Set(LocoSpeedSteps.Steps126, 55)
        };
        var json = JsonSerializer.Serialize(target, JsonSerializationOptions.Default);
        var actual = JsonSerializer.Deserialize<Drive>(json, JsonSerializationOptions.Default);

        Assert.IsNotNull(actual);
        Assert.AreEqual(target.Direction, actual.Direction);
        Assert.AreEqual(target.Speed.CurrentStep, actual.Speed.CurrentStep);
    }

    [TestMethod]
    public void LocoAddress_SerializesAsNumber()
    {
        var address = LocoAddress.From(42);
        var json = System.Text.Json.JsonSerializer.Serialize(address, JsonSerializationOptions.Default);
        Assert.AreEqual("42", json);
    }

    [TestMethod]
    public void LocoAddress_DeserializesFromNumber()
    {
        var json = "123";
        var address = System.Text.Json.JsonSerializer.Deserialize<LocoAddress>(json, JsonSerializationOptions.Default);
        Assert.AreEqual(123, address.Number);
    }

    [TestMethod]
    public void Speed_SerializesAsObject()
    {
        var speed = Speed.Set126(75);
        var json = System.Text.Json.JsonSerializer.Serialize(speed, JsonSerializationOptions.Default);
        Assert.IsTrue(json.Contains("\"maxSteps\":126"));
        Assert.IsTrue(json.Contains("\"currentStep\":75"));
    }

    [TestMethod]
    public void Speed_DeserializesFromObject()
    {
        var json = "{\"maxSteps\":126,\"currentStep\":75}";
        var speed = System.Text.Json.JsonSerializer.Deserialize<Speed>(json, JsonSerializationOptions.Default);
        Assert.AreEqual(LocoSpeedSteps.Steps126, speed.MaxSteps);
        Assert.AreEqual(75, speed.CurrentStep);
    }

    [TestMethod]
    public void Function_SerializesAsObject()
    {
        var function = Function.On(Functions.F5);
        var json = System.Text.Json.JsonSerializer.Serialize(function, JsonSerializationOptions.Default);
        Assert.IsTrue(json.Contains("\"number\":\"F5\""));
        Assert.IsTrue(json.Contains("\"isOn\":true"));
    }

    [TestMethod]
    public void Function_DeserializesFromObject()
    {
        var json = "{\"number\":\"F5\",\"isOn\":true}";
        var function = System.Text.Json.JsonSerializer.Deserialize<Function>(json, JsonSerializationOptions.Default);
        Assert.AreEqual(Functions.F5, function.Number);
        Assert.IsTrue(function.IsOn);
    }

    [TestMethod]
    public void LocoMovementNotification_SerializesWithTypeDiscriminator()
    {
        Notification notification = new LocoMovementNotification(
            LocoAddress.From(42),
            Direction.Forward,
            Speed.Set126(75));

        var json = System.Text.Json.JsonSerializer.Serialize(notification, JsonSerializationOptions.Default);

        Assert.IsTrue(json.Contains("\"$type\":\"LocoMovementNotification\""));
        Assert.IsTrue(json.Contains("\"address\":42"));
        Assert.IsTrue(json.Contains("\"direction\":\"forward\""));
    }

    [TestMethod]
    public void LocoMovementNotification_DeserializesToCorrectType()
    {
        var json = "{\"$type\":\"LocoMovementNotification\",\"address\":42,\"direction\":\"forward\",\"speed\":{\"maxSteps\":126,\"currentStep\":75}}";
        var notification = System.Text.Json.JsonSerializer.Deserialize<Notification>(json, JsonSerializationOptions.Default);

        Assert.IsInstanceOfType(notification, typeof(LocoMovementNotification));
        var loco = (LocoMovementNotification)notification!;
        Assert.AreEqual(42, loco.Address.Number);
        Assert.AreEqual(Direction.Forward, loco.Direction);
        Assert.AreEqual(75, loco.Speed.CurrentStep);
    }

    [TestMethod]
    public void AccessoryNotification_SerializesWithTypeDiscriminator()
    {
        Notification notification = new AccessoryNotification(
            AccessoryAddress.From(10),
            Position.ThrownOrRed);

        var json = System.Text.Json.JsonSerializer.Serialize(notification, JsonSerializationOptions.Default);

        Assert.IsTrue(json.Contains("\"$type\":\"AccessoryNotification\""));
        Assert.IsTrue(json.Contains("\"address\":10"));
        Assert.IsTrue(json.Contains("\"function\":\"thrownOrRed\""));
    }

    [TestMethod]
    public void AccessoryNotification_DeserializesToCorrectType()
    {
        var json = "{\"$type\":\"AccessoryNotification\",\"address\":10,\"function\":\"thrownOrRed\"}";
        var notification = System.Text.Json.JsonSerializer.Deserialize<Notification>(json, JsonSerializationOptions.Default);

        Assert.IsInstanceOfType(notification, typeof(AccessoryNotification));
        var accessory = (AccessoryNotification)notification!;
        Assert.AreEqual(10, accessory.Address.Number);
        Assert.AreEqual(Position.ThrownOrRed, accessory.Function);
    }

    [TestMethod]
    public void ShortCircuitNotification_SerializesWithTypeDiscriminator()
    {
        Notification notification = new ShortCircuitNotification();

        var json = System.Text.Json.JsonSerializer.Serialize(notification, JsonSerializationOptions.Default);

        Assert.IsTrue(json.Contains("\"$type\":\"ShortCircuitNotification\""));
    }

    [TestMethod]
    public void MessageNotification_SerializesWithMessage()
    {
        Notification notification = new MessageNotification(DateTimeOffset.Now, "Test message");

        var json = System.Text.Json.JsonSerializer.Serialize(notification, JsonSerializationOptions.Default);

        Assert.IsTrue(json.Contains("\"$type\":\"MessageNotification\""));
        Assert.IsTrue(json.Contains("\"message\":\"Test message\""));
    }

    [TestMethod]
    public void LocoFunctionsNotification_SerializesWithFunctions()
    {
        Notification notification = new LocoFunctionsNotification(
            LocoAddress.From(42),
            [Function.On(Functions.F0), Function.On(Functions.F5)]);

        var json = System.Text.Json.JsonSerializer.Serialize(notification, JsonSerializationOptions.Default);

        Assert.IsTrue(json.Contains("\"$type\":\"LocoFunctionsNotification\""));
        Assert.IsTrue(json.Contains("\"activeFunctions\""));
    }

    [TestMethod]
    public void CreateIndented_ReturnsFormattedJson()
    {
        var options = JsonSerializationOptions.CreateIndented();
        var notification = new ShortCircuitNotification();
        var json = System.Text.Json.JsonSerializer.Serialize(notification, options);

        // Indented JSON should contain newlines
        Assert.IsTrue(json.Contains("\n") || json.Contains("\r\n"));
    }
}
