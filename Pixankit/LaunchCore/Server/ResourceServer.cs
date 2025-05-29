using System.Net.NetworkInformation;

namespace PixanKit.LaunchCore.Server;

/// <summary>
/// Represents an abstract resource server that manages mirror servers
/// and handles server selection based on ping times.
/// </summary>
public abstract class ResourceServer
{
    /// <summary>
    /// The list of available mirror servers.
    /// </summary>
    public List<MirrorServer> Mirrors = [];

    /// <summary>
    /// Current Mirror Server
    /// </summary>
    public MirrorServer? Current { get; set; }

    /// <summary>
    /// Selects the best mirror server from the list based on ping times.
    /// </summary>
    public void UpdateIndex()
    {
        List<KeyValuePair<long, MirrorServer>> dict = [];
        foreach (var item in Mirrors)
        {
            try
            {
                var pingtime = GetPing(item.BaseUrl);
                if (pingtime > 0)
                    dict.Add(new(pingtime, item));
            }
            catch
            {
                Current = Mirrors[0];
                return;
            }
        }
        dict.Sort((a, b) => a.Key.CompareTo(b.Key));
        Current = dict.First().Value;
    }

    /// <summary>
    /// Selects a specific mirror server from the list based on its index.
    /// </summary>
    /// <param name="index">The index of the server in the <see cref="Mirrors"/> list.</param>
    public void UpdateIndex(int index)
    {
        Current = Mirrors[index];
    }

    /// <summary>
    /// Gets the ping time for the current mirror server.
    /// </summary>
    /// <returns>The round-trip time in milliseconds to ping the current server, or -1 if the ping fails.</returns>
    public long Ping()
    {
        if (Current == null) throw new Exception();
        return GetPing(Current.BaseUrl);
    }

    /// <summary>
    /// Gets the ping time for a specific server URL.
    /// </summary>
    /// <param name="url">The URL of the server to ping. The method will automatically extract the host from the URL.</param>
    /// <returns>The round-trip time in milliseconds to ping the server, or -1 if the ping fails.</returns>
    public static long GetPing(string url)
    {
        Ping ping = new();
        var reply = ping.Send(GetHost(url));
        if (reply.Status == IPStatus.Success) return reply.RoundtripTime;
        else return -1;
    }

    /// <summary>
    /// Adds a new mirror server to the list of available servers.
    /// </summary>
    /// <param name="server">The <see cref="MirrorServer"/> to add.</param>
    public void Add(MirrorServer server)
    {
        Mirrors.Add(server);
    }

    /// <summary>
    /// Replaces the Vanilla URL of a resource with the corresponding URL from the current mirror server.
    /// </summary>
    /// <param name="originalUrl">The Vanilla URL of the resource.</param>
    /// <returns>The URL of the resource on the current mirror server.</returns>
    protected string Replace(string originalUrl)
    {
        if (Current == null) throw new Exception();
        return Current.Replace(originalUrl);
    }

    private static string GetHost(string originalUrl)
    {

        Uri uri = new(originalUrl);
        return uri.Host;

    }
}