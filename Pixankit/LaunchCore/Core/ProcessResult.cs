namespace PixanKit.LaunchCore.Core
{
    /// <summary>
    /// Represents the result of a Minecraft process.
    /// </summary>
    public struct ProcessResult
    {
        /// <summary>
        /// Gets or sets the return code of the Minecraft process.
        /// </summary>
        public int ReturnCode;

        /// <summary>
        /// Gets a value indicating whether the process exited successfully.
        /// </summary>
        public readonly bool Successful => ReturnCode == 0;

        /// <summary>
        /// Gets or sets the path to the log file, which is a .tar.gz archive.
        /// </summary>
        public string LogGZPath;

        /// <summary>
        /// Gets or sets the path to the crash file, if applicable.
        /// </summary>
        public string? CrashFilePath;

        /// <summary>
        /// Gets or sets the output stream of the Minecraft process.
        /// </summary>
        public Stream OutputStream;

        /// <summary>
        /// Gets or sets the output stream of the Minecraft process.
        /// </summary>
        public Stream ErrorStream;

        /// <summary>
        /// Closes the output stream associated with the Minecraft process.
        /// </summary>
        public readonly void Close()
        {
            OutputStream.Close();
        }
    }
}
