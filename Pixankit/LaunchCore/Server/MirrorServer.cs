namespace PixanKit.LaunchCore.Server;

/// <summary>
/// Mirror Server
/// </summary>
public class MirrorServer
{
    /// <summary>
    /// The Vanilla base url of the network assets
    /// </summary>
    protected string OriginalUrl;

    /// <summary>
    /// The base url of the network assets. It will replace the base url
    /// </summary>
    public string BaseUrl;

    /// <summary>
    /// Initor But Do Nothing. Do Not Use That
    /// </summary>
    public MirrorServer() { OriginalUrl = ""; BaseUrl = ""; }

    ///<summary>Initor</summary>
    /// <param name="originalUrl">The Url Part That Needs To Replace(If No, Make It "")</param>
    /// <param name="replacedUrl">The Url That Replacing(Include https://)</param>
    public MirrorServer(string originalUrl, string replacedUrl)
    {
        this.OriginalUrl = originalUrl;
        this.BaseUrl = replacedUrl;
    }

    /// <summary>
    /// Replace The URL
    /// </summary>
    /// <param name="url">The Vanilla URL</param>
    /// <returns>The Replaced URL</returns>
    public virtual string Replace(string url)
    {
        return OriginalUrl == "" ? url : url.Replace(OriginalUrl, BaseUrl);
    }
}