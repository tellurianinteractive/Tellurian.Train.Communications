using System.Globalization;

namespace Tellurian.Trains.Protocols.XpressNet.Notifications;

public sealed class LocoInfoNotification : Notification
{
    public LocoInfoNotification(byte[] buffer) : base(0xE0, GetData(buffer)) { }

    public LocoAddress Address { get { return new LocoAddress(Data.Take(2).ToArray()); } }
    public LocoDirection Direction { get { return (Data[3] & 0x80) > 0 ? LocoDirection.Forward : LocoDirection.Backward; } }

    public bool IsControlledByOtherDevice { get { return (Data[2] & 0x08) > 0; } }

    public LocoSpeed Speed
    {
        get
        {
            var value = (byte)(Data[2] & 0x07);
            var speed = LocoSpeed.FromCode(value);
            speed.Current = (byte)(Data[3] & 0x7F);
            return speed;
        }
    }

    public override string ToString() =>
        string.Format(CultureInfo.InvariantCulture, "{0} Address={1} Speed={2}", base.ToString(), Address, Speed);

    public (int, bool)[] Functions() =>
        new (int, bool)[]  {
           (0, (Data[4] & 0b0001_0000) > 0),
           (1, (Data[4] & 0b0000_0001) > 0),
           (2, (Data[4] & 0b0000_0010) > 0),
           (3, (Data[4] & 0b0000_0100) > 0),
           (4, (Data[4] & 0b0000_1000) > 0),
           (5, (Data[5] & 0b0000_0001) > 0),
           (6, (Data[5] & 0b0000_0010) > 0),
           (7, (Data[5] & 0b0000_0100) > 0),
           (8, (Data[5] & 0b0000_1000) > 0),
           (9, (Data[5] & 0b0001_0000) > 0),
           (10, (Data[5] & 0b0010_0000) > 0),
           (11, (Data[5] & 0b0100_0000) > 0),
           (12, (Data[5] & 0b1000_0000) > 0),
           (13, (Data[6] & 0b0000_0001) > 0),
           (14, (Data[6] & 0b0000_0010) > 0),
           (15, (Data[6] & 0b0000_0100) > 0),
           (16, (Data[6] & 0b0000_1000) > 0),
           (17, (Data[6] & 0b0001_0000) > 0),
           (18, (Data[6] & 0b0010_0000) > 0),
           (19, (Data[6] & 0b0100_0000) > 0),
           (20, (Data[6] & 0b1000_0000) > 0),
           (21, (Data[7] & 0b0000_0001) > 0),
           (22, (Data[7] & 0b0000_0010) > 0),
           (23, (Data[7] & 0b0000_0100) > 0),
           (24, (Data[7] & 0b0000_1000) > 0),
           (25, (Data[7] & 0b0001_0000) > 0),
           (26, (Data[7] & 0b0010_0000) > 0),
           (27, (Data[7] & 0b0100_0000) > 0),
           (28, (Data[7] & 0b1000_0000) > 0)
        };
        }
