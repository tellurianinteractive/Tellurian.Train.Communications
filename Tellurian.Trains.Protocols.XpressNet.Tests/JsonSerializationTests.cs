using System.Text.Json;
using Tellurian.Trains.Protocols.XpressNet.Json;
using Tellurian.Trains.Protocols.XpressNet.Notifications;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class JsonSerializationTests
{
    [TestMethod]
    public void LocoInfoNotification_SerializesToJson()
    {
        // 0xEF = Message header for LocoInfoNotification (0xE0 is the parsed header internally)
        // Address high: 0x00, Address low: 0x05 = Address 5
        // Status byte with speed steps
        // Speed + direction
        // Functions F0-F28
        byte[] buffer = [0xEF, 0x00, 0x05, 0x04, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00];
        var notification = new LocoInfoNotification(buffer);

        var json = JsonSerializer.Serialize<Message>(notification, XpressNetJsonSerializationOptions.Default);

        Assert.IsTrue(json.Contains("\"$type\":\"LocoInfoNotification\""));
        Assert.IsTrue(json.Contains("\"header\":224")); // 0xE0 = 224 (the internal parsed header)
    }

    [TestMethod]
    public void TrackPowerOnBroadcast_SerializesToJson()
    {
        var notification = new TrackPowerOnBroadcast();

        var json = JsonSerializer.Serialize<Message>(notification, XpressNetJsonSerializationOptions.Default);

        Assert.IsTrue(json.Contains("\"$type\":\"TrackPowerOnBroadcast\""));
    }

    [TestMethod]
    public void TrackPowerOffBroadcast_SerializesToJson()
    {
        var notification = new TrackPowerOffBroadcast();

        var json = JsonSerializer.Serialize<Message>(notification, XpressNetJsonSerializationOptions.Default);

        Assert.IsTrue(json.Contains("\"$type\":\"TrackPowerOffBroadcast\""));
    }

    [TestMethod]
    public void EmergencyStopBroadcast_SerializesToJson()
    {
        var notification = new EmergencyStopBroadcast();

        var json = JsonSerializer.Serialize<Message>(notification, XpressNetJsonSerializationOptions.Default);

        Assert.IsTrue(json.Contains("\"$type\":\"EmergencyStopBroadcast\""));
    }

    [TestMethod]
    public void LocoSpeed_SerializesToJson()
    {
        var speed = LocoSpeed.FromNumberOfSteps(126, 75);

        var json = JsonSerializer.Serialize(speed, XpressNetJsonSerializationOptions.Default);

        Assert.IsTrue(json.Contains("\"maxSteps\":126"));
        Assert.IsTrue(json.Contains("\"current\":75"));
    }

    [TestMethod]
    public void LocoSpeed_DeserializesFromJson()
    {
        var json = "{\"maxSteps\":126,\"current\":75}";

        var speed = JsonSerializer.Deserialize<LocoSpeed>(json, XpressNetJsonSerializationOptions.Default);

        Assert.AreEqual(126, speed.MaxSteps);
        Assert.AreEqual(75, speed.Current);
    }

    [TestMethod]
    public void CreateIndented_ReturnsFormattedJson()
    {
        var options = XpressNetJsonSerializationOptions.CreateIndented();
        var notification = new TrackPowerOnBroadcast();

        var json = JsonSerializer.Serialize<Message>(notification, options);

        // Indented JSON should contain newlines
        Assert.IsTrue(json.Contains("\n") || json.Contains("\r\n"));
    }
}
