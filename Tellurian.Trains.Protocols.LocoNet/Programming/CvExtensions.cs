using Tellurian.Trains.Communications.Interfaces.Decoder;

namespace Tellurian.Trains.Protocols.LocoNet.Programming;

/// <summary>
/// LocoNet-specific CV encoding/decoding extensions.
/// These handle the CVH/CVL/DATA7 byte encoding used in LocoNet programming messages.
/// </summary>
public static partial class CvExtensions
{
    extension(CV cv)
    {
        /// <summary>
        /// Encodes a CV number (1-1024) and value into CVH, CVL, and DATA7 bytes for LocoNet.
        /// </summary>
        /// <returns>Tuple of (cvh, cvl, data7)</returns>
        public (byte cvh, byte cvl, byte data7) EncodeToBytes()
        {
            // CV numbers are 1-indexed, but we transmit 0-indexed
            int cvIndex = cv.Number - 1;

            // CVL: bits 6-0 of CV index
            byte cvl = (byte)(cvIndex & 0x7F);

            // CVH: bit 0 = CV bit 7, bit 1 = data bit 7
            byte cvh = (byte)((cvIndex >> 7) & 0x01);

            // Data bit 7 goes into CVH bit 1
            if ((cv.Value & 0x80) != 0)
                cvh |= 0x02;

            // DATA7: bits 6-0 of data value
            byte data7 = (byte)(cv.Value & 0x7F);

            return (cvh, cvl, data7);
        }

        /// <summary>
        /// Decodes CVH, CVL, and DATA7 bytes back to a CV struct.
        /// </summary>
        /// <param name="cvh">CVH byte</param>
        /// <param name="cvl">CVL byte</param>
        /// <param name="data7">DATA7 byte</param>
        /// <returns>CV struct with decoded number and value</returns>
        public static CV DecodeFromBytes(byte cvh, byte cvl, byte data7)
        {
            // Reconstruct CV index from CVL (bits 6-0) and CVH bit 0 (bit 7)
            int cvIndex = cvl | ((cvh & 0x01) << 7);

            // CV numbers are 1-indexed
            int cvNumber = cvIndex + 1;

            // Reconstruct data value from DATA7 (bits 6-0) and CVH bit 1 (bit 7)
            byte cvValue = (byte)(data7 | ((cvh & 0x02) << 6));

            return new CV(cvNumber, cvValue);
        }
    }
}
