namespace Tellurian.Trains.Communications.Interfaces.Decoder;

public interface IDecoder
{
    public Task<byte> ReadCVAsync(ushort number, CancellationToken cancellationToken = default);
    public Task WriteCVAsync(ushort number, byte value, CancellationToken cancellationToken = default);
}
