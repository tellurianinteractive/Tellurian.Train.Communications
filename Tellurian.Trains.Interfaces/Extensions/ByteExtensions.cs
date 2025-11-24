using System.Buffers.Binary;

namespace Tellurian.Trains.Interfaces.Extensions;

public static class ByteExtensions
{
    extension(byte value)
    {
        public string AsHex()
        {
            var chars = new char[2];
            byte temp = ((byte)(value >> 4));
            chars[0] = (char)(temp > 9 ? temp + 0x37 + 0x20 : temp + 0x30);
            temp = ((byte)(value & 0x0F));
            chars[1] = (char)(temp > 9 ? temp + 0x37 + 0x20 : temp + 0x30);
            return new string(chars);
        }

        public int Bcd() => ((value & 0xF0) / 16 * 10) + (value & 0x0F);
    }

    extension(byte[] bytes)
    {
        public string AsHex()
        {
            if (bytes is null || bytes.Length == 0) return string.Empty;
            char[] chars = new char[bytes.Length * 2];
            byte temp;
            for (int bx = 0, cx = 0; bx < bytes.Length; ++bx, ++cx)
            {
                temp = ((byte)(bytes[bx] >> 4));
                chars[cx] = (char)(temp > 9 ? temp + 0x37 + 0x20 : temp + 0x30);
                temp = ((byte)(bytes[bx] & 0x0F));
                chars[++cx] = (char)(temp > 9 ? temp + 0x37 + 0x20 : temp + 0x30);
            }
            return new string(chars);
        }
    }

    extension(int value)
    {
        public byte[] ToBigEndian() =>
            BitConverter.GetBytes(BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value);

        public byte[] ToLittleEndian() =>
            BitConverter.GetBytes(BitConverter.IsLittleEndian ? value : BinaryPrimitives.ReverseEndianness(value));
    }

    extension(uint value)
    {
        public byte[] ToBigEndian() =>
        BitConverter.GetBytes(BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value);

        public byte[] ToLittleEndian() =>
            BitConverter.GetBytes(BitConverter.IsLittleEndian ? value : BinaryPrimitives.ReverseEndianness(value));
    }
    extension(byte[] buffer)
    {
        public ushort ToUint16LittleEndian(int offset = 0) =>
            BitConverter.IsLittleEndian ? BitConverter.ToUInt16(buffer, offset) : BinaryPrimitives.ReadUInt16BigEndian(buffer.AsSpan().Slice(offset, 2));

        public short ToInt16LittleEndian(int offset = 0) =>
            BitConverter.IsLittleEndian ? BitConverter.ToInt16(buffer, offset) : BinaryPrimitives.ReadInt16BigEndian(buffer.AsSpan().Slice(offset, 2));

        public ushort ToUint16BigEndian(int offset = 0) =>
            BitConverter.IsLittleEndian ? BinaryPrimitives.ReadUInt16BigEndian(buffer.AsSpan().Slice(offset, 2)) : BitConverter.ToUInt16(buffer, offset);

        public short ToInt16BigEndian(int offset = 0) =>
            BitConverter.IsLittleEndian ? BinaryPrimitives.ReadInt16BigEndian(buffer.AsSpan().Slice(offset, 2)) : BitConverter.ToInt16(buffer, offset);

        public uint ToUint32LittleEndian(int offset = 0) =>
            BitConverter.IsLittleEndian ? BitConverter.ToUInt32(buffer, offset) : BinaryPrimitives.ReadUInt32BigEndian(buffer.AsSpan().Slice(offset, 4));

        public int ToInt32LittleEndian(int offset = 0) =>
            BitConverter.IsLittleEndian ? BitConverter.ToInt32(buffer, offset) : BinaryPrimitives.ReadInt32BigEndian(buffer.AsSpan().Slice(offset, 4));

        public uint ToUint32BigEndian(int offset = 0) =>
            BitConverter.IsLittleEndian ? BinaryPrimitives.ReadUInt32BigEndian(buffer.AsSpan().Slice(offset, 4)) : BitConverter.ToUInt32(buffer, offset);

        public int ToInt32BigEndian(int offset = 0) =>
            BitConverter.IsLittleEndian ? BinaryPrimitives.ReadInt32BigEndian(buffer.AsSpan().Slice(offset, 4)) : BitConverter.ToInt32(buffer, offset);
    }
}
