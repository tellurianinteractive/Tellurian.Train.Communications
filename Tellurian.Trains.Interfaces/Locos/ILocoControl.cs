namespace Tellurian.Trains.Interfaces.Locos;

public interface ILocoControl
{
    bool Drive(LocoAddress address, LocoDrive drive);
    bool EmergencyStop(LocoAddress address);
    bool SetFunction(LocoAddress address, LocoFunction locoFunction);
}
