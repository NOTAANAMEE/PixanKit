namespace PixanKit.LaunchCore.Core.LaunchSession;

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
    public string LogGzPath;
}