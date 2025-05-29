namespace PixanKit.LaunchCore.Logger;

/// <summary>
/// Logger Output Class
/// </summary>
public static class Logger
{
    static Logger()
    {
        Output = Console.Out;
    }

    /// <summary>
    /// The output of the logger. The logger will use this writer for log
    /// </summary>
    public static TextWriter Output;

    /// <summary>
    /// Add A Info Message
    /// </summary>
    /// <param name="from">The Package Name</param>
    /// <param name="message">The Message</param>
    public static void Info(string from, string message)
    {
        Record(from, "Info", message);
    }

    /// <summary>
    /// Add A Warn Message
    /// </summary>
    /// <param name="from">The Package Name</param>
    /// <param name="message">The Message</param>
    public static void Warn(string from, string message)
    {
        Record(from, "Warn", message);
    }

    /// <summary>
    /// Add An Error Message
    /// </summary>
    /// <param name="from">The Package Name</param>
    /// <param name="message">The Message</param>
    public static void Error(string from, string message)
    {
        Record(from, "Error", message);
    }

    internal static void Info(string message)
    {
        Info("PixanKit.LaunchCore", message);
    }

    internal static void Warn(string message)
    {
        Warn("PixanKit.LaunchCore", message);
    }

    internal static void Error(string message)
    {
        Error("PixanKit.LaunchCore", message);
    }

    private static void Record(string from, string type, string message)
    {
        var log = $"[{DateTime.Now} {from}] [{type}] {message}";
        Output.WriteLine(log);
    }
}