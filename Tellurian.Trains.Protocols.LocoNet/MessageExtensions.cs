using Tellurian.Trains.Communications.Interfaces.Detectors;
using Tellurian.Trains.Communications.Interfaces.Extensions;
using Tellurian.Trains.Protocols.LocoNet.Notifications;

namespace Tellurian.Trains.Protocols.LocoNet;

public static class MessageExtensions
{
    extension(Message message)
    {
        public Tellurian.Trains.Communications.Interfaces.Notification[] Map =>
                message switch
                {
                    SensorInputNotification sensor => [new OccupancyNotification(sensor.Address, sensor.IsOccupied)],
                    MultiSenseNotification ms when ms.IsTransponding => [new TransponderNotification(ms.Section, ms.LocoAddress, ms.IsPresent)],
                    LissyNotification lissy when lissy.IsValid => [new RailComLocomotiveNotification(lissy.SectionAddress, lissy.LocoAddress, true, lissy.IsForward, lissy.Category)],
                    null => [],
                    _ => MapDefaults.CreateUnmapped(message.ToString()),
                };
    }

}
