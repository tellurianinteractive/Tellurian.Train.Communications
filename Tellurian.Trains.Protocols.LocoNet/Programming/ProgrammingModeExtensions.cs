namespace Tellurian.Trains.Protocols.LocoNet.Programming;

/// <summary>
/// Helper methods for building PCMD byte and encoding CV/data values.
/// </summary>
public static class ProgrammingModeExtensions
{
    extension(ProgrammingMode mode)
    {
        /// <summary>
        /// Builds the PCMD (programming command) byte from programming parameters.
        /// </summary>
        /// <param name="mode">Programming mode</param>
        /// <param name="operation">Read or Write</param>
        /// <returns>PCMD byte value</returns>
        public  byte BuildProgrammingCommandByte(ProgrammingOperation operation)
        {
            byte programmingCommandByte = 0;

            // Bit 6: Write/Read (1=Write, 0=Read)
            if (operation == ProgrammingOperation.Write)
                programmingCommandByte |= 0x40;

            // Bit 5: Byte mode (1=Byte operation, 0=Bit operation)
            // Bit 4: TY1
            // Bit 3: TY0
            // Bit 2: Ops mode (1=Operations mode, 0=Service mode)

            programmingCommandByte |= mode switch
            {
                ProgrammingMode.PagedModeService => 0b00100000, // byte=1, ops=0, TY=00
                ProgrammingMode.DirectModeByteService => 0b00101000, // byte=1, ops=0, TY=01
                ProgrammingMode.DirectModeBitService => 0b00001000, // byte=0, ops=0, TY=01
                ProgrammingMode.RegisterModeService => 0b00110000, // byte=x, ops=0, TY=10
                ProgrammingMode.OperationsModeByte => 0b00100100, // byte=1, ops=1, TY=00
                ProgrammingMode.OperationsModeByteWithFeedback => 0b00101100, // byte=1, ops=1, TY=01
                ProgrammingMode.OperationsModeBit => 0b00000100, // byte=0, ops=1, TY=00
                ProgrammingMode.OperationsModeBitWithFeedback => 0b00001100, // byte=0, ops=1, TY=01
                _ => throw new ArgumentException("Invalid programming mode")
            };

            return programmingCommandByte;
        }
    }
}