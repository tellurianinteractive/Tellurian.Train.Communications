using Tellurian.Trains.Communications.Interfaces.Extensions;

namespace Tellurian.Trains.Protocols.LocoNet;

public static class MessageExtensions
{
    extension(Message message)
    {
        public Tellurian.Trains.Communications.Interfaces.Notification[] Map =>
                message is null ?
                [] :
                MapDefaults.CreateUnmapped(message.ToString()); // We do not support LocoNet notifications yet.
    }

}
