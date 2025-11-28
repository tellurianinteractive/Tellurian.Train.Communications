namespace Tellurian.Trains.Protocols.LocoNet.Programming;

public static class ProgrammingStatusExtensions
{
    extension(ProgrammingStatus status)
    {
        /// <summary>
        /// Checks if programming status indicates success.
        /// </summary>
        public bool IsSuccess()
        {
            return status == ProgrammingStatus.Success;
        }

        /// <summary>
        /// Gets a human-readable error message for programming status.
        /// </summary>
        public string GetMessage()
        {
            if (status == ProgrammingStatus.Success)
                return "Programming succeeded";

            var messages = new List<string>();

            if ((status & ProgrammingStatus.NoDecoder) != 0)
                messages.Add("No decoder detected on programming track");

            if ((status & ProgrammingStatus.WriteAckFail) != 0)
                messages.Add("Write acknowledge failed");

            if ((status & ProgrammingStatus.ReadAckFail) != 0)
                messages.Add("Read acknowledge failed");

            if ((status & ProgrammingStatus.UserAborted) != 0)
                messages.Add("User aborted operation");

            return string.Join("; ", messages);
        }

    }
}
