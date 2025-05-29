using PixanKit.LaunchCore.Logger;
using PixanKit.ResourceDownloader.Tasks.FuncTask;

namespace PixanKit.ResourceDownloader.Download.DownloadTask;

/// <summary>
/// Represents a task for downloading a specific chunk of a file from a given URL.
/// </summary>
public class FileChunkDownloadTask : FuncProgressTask<Stream>, IFileDownload
{
    /// <summary>
    /// The default size of a file chunk in bytes.
    /// </summary>
    public static readonly long ChunkSize = 1024 * 1024;

    private readonly string _url;

    /// <inheritdoc/>
    public long Size => End - StartPosition + 1;

    /// <inheritdoc/>
    public long DownloadedBytes => _downloadedBytes;

    /// <inheritdoc/>
    public int TotalFiles => 0;

    /// <inheritdoc/>
    public int DownloadedFiles => 0;

    /// <summary>
    /// The starting byte position of the file chunk to download.
    /// </summary>
    public readonly long StartPosition;

    /// <summary>
    /// The ending byte position of the file chunk to download.
    /// </summary>
    public readonly long End;

    long _downloadedBytes;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileChunkDownloadTask"/> class 
    /// with a specified URL, starting byte position, and ending byte position.
    /// </summary>
    /// <param name="url">The URL of the file to download.</param>
    /// <param name="startPosition">The starting byte position of the chunk.</param>
    /// <param name="end">The ending byte position of the chunk.</param>
    public FileChunkDownloadTask(string url, long startPosition, long end)
    {
        _url = url;
        StartPosition = startPosition;
        End = end;
        Function += DownloadAsync;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileChunkDownloadTask"/> class 
    /// with a specified URL and starting byte position. The chunk size is set to the default value.
    /// </summary>
    /// <param name="url">The URL of the file to download.</param>
    /// <param name="startPosition">The starting byte position of the chunk.</param>
    public FileChunkDownloadTask(string url, long startPosition)
    {
        _url = url;
        StartPosition = startPosition;
        End = startPosition + ChunkSize - 1;
        Function += DownloadAsync;
    }

    private async Task<Stream> DownloadAsync(Action<double> progress, CancellationToken token)
    {
        HttpClient client = new();
        MemoryStream ret = new();
        HttpResponseMessage? response = null;
        client.DefaultRequestHeaders.Range = new System.Net.Http.Headers.RangeHeaderValue(StartPosition, End);
        try
        {
            response = await client.GetAsync(_url, HttpCompletionOption.ResponseHeadersRead, CancellationToken.Token);
            response.EnsureSuccessStatusCode();
            await LoopRead(response.Content, ret, progress);
        }
        catch (Exception ex)
        {
            response?.Dispose();
            Logger.Warn("PixanKit.ResourceDownloader", ex.Message);
        }
        client.Dispose();

        return ret;
    }

    private async Task LoopRead(HttpContent content, Stream ret, Action<double> progress)
    {
        int bytesRead;
        var stream = await content.ReadAsStreamAsync();
        var buffer = new byte[8192];
        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, CancellationToken.Token)) > 0
               && !CancellationToken.IsCancellationRequested)
        {
            await ret.WriteAsync(buffer, 0, bytesRead);
            _downloadedBytes += bytesRead;
            progress((double)_downloadedBytes / Size);
        }
    }
}