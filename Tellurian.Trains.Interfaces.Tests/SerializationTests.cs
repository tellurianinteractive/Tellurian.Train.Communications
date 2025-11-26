using Newtonsoft.Json;
using Tellurian.Trains.Interfaces.Locos;

namespace Tellurian.Trains.Interfaces.Tests;

[TestClass]
public class SerializationTests
{
    [TestMethod]
    public void LocoDrive_SerializesAndDeserializes_WithJson()
    {
        var target = new LocoDrive()
        {
            Direction = LocoDirection.Forward,
            Speed = LocoSpeed.Set(LocoSpeedSteps.Steps126, 55)
        };
        var serializer = new JsonSerializer();
        using var s = new MemoryStream();
        using var w = new StreamWriter(s);
        serializer.Serialize(w, target);
        w.Flush();
        s.Position = 0;
        using var r = new StreamReader(s);
        var actual = serializer.Deserialize(r, typeof(LocoDrive));
    }
}
