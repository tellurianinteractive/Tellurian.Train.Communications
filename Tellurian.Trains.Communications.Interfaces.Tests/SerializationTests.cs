using System.Text.Json;
using Tellurian.Trains.Communications.Interfaces.Accessories;
using Tellurian.Trains.Communications.Interfaces.Json;
using Tellurian.Trains.Communications.Interfaces.Locos;
using LocoAddress = Tellurian.Trains.Communications.Interfaces.Locos.Address;
using AccessoryAddress = Tellurian.Trains.Communications.Interfaces.Accessories.Address;

namespace Tellurian.Trains.Communications.Interfaces.Tests;

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
        StringAssert.Contains(json, "\"maxSteps\":126");
        StringAssert.Contains(json, "\"currentStep\":75");
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
        StringAssert.Contains(json, "\"number\":\"F5\"");
        StringAssert.Contains(json, "\"isOn\":true");
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

        StringAssert.Contains(json, "\"$type\":\"LocoMovementNotification\"");
        StringAssert.Contains(json, "\"address\":42");
        StringAssert.Contains(json, "\"direction\":\"forward\"");
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

        StringAssert.Contains(json, "\"$type\":\"AccessoryNotification\"");
        StringAssert.Contains(json, "\"address\":10");
        StringAssert.Contains(json, "\"function\":\"thrownOrRed\"");
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

        StringAssert.Contains(json, "\"$type\":\"ShortCircuitNotification\"");
    }

    [TestMethod]
    public void MessageNotification_SerializesWithMessage()
    {
        Notification notification = new MessageNotification(DateTimeOffset.Now, "Test message");

        var json = System.Text.Json.JsonSerializer.Serialize(notification, JsonSerializationOptions.Default);

        StringAssert.Contains(json, "\"$type\":\"MessageNotification\"");
        StringAssert.Contains(json, "\"message\":\"Test message\"");
    }

    [TestMethod]
    public void LocoFunctionsNotification_SerializesWithFunctions()
    {
        Notification notification = new LocoFunctionsNotification(
            LocoAddress.From(42),
            [Function.On(Functions.F0), Function.On(Functions.F5)]);

        var json = System.Text.Json.JsonSerializer.Serialize(notification, JsonSerializationOptions.Default);

        StringAssert.Contains(json, "\"$type\":\"LocoFunctionsNotification\"");
        StringAssert.Contains(json, "\"activeFunctions\"");
    }

    [TestMethod]
    public void CreateIndented_ReturnsFormattedJson()
    {
        var options = JsonSerializationOptions.CreateIndented();
        var notification = new ShortCircuitNotification();
        var json = System.Text.Json.JsonSerializer.Serialize(notification, options);

        // Indented JSON should contain newlines
        StringAssert.Contains(json, "\n");
    }
}
