namespace PixanKit.LaunchCore.GameModule.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a Folder instance is not found.
/// </summary>
public class NoFolderException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoFolderException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public NoFolderException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="NoFolderException"/> class with a default error message.
    /// </summary>
    public NoFolderException() : base("The required folder was not found.") { }
}