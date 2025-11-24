namespace Tellurian.Trains.Interfaces.Decoder;

public interface ILocoDecoder
{
    public byte ReadCV(ushort number);
    public void WriteCV(ushort number, byte value);
}
