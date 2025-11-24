using System.Globalization;

namespace Tellurian.Trains.Adapters.Z21;

public abstract class AddressModeNotification : Notification
{
    protected AddressModeNotification(Frame frame) : base(frame)
    {
        if (frame is null) throw new ArgumentNullException(nameof(frame));
        Address = BitConverterExtensions.GetBigEndianInt16(frame.Data, 0);
        Mode = (AddressMode)frame.Data[2];
    }
    public short Address { get; }
    public AddressMode Mode { get; }

    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}: Address={1}, Mode={2}", base.ToString(), Address, Mode);
    }
}
