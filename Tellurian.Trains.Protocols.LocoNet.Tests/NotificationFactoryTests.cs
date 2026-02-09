using Tellurian.Trains.Communications.Interfaces.Accessories;
using Tellurian.Trains.Protocols.LocoNet.Commands;
using Tellurian.Trains.Protocols.LocoNet.Notifications;

namespace Tellurian.Trains.Protocols.LocoNet.Tests;

[TestClass]
public class NotificationFactoryTests
{
    [TestMethod]
    public void Factory_Creates_SetAccessoryNotification_FromOpcodeB0()
    {
        var command = SetAccessoryCommand.Close(Address.From(1));
        var bytes = command.GetBytesWithChecksum();

        var message = LocoNetMessageFactory.Create(bytes);

        Assert.IsInstanceOfType<SetAccessoryNotification>(message);
    }
}
