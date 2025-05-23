using PixanKit.LaunchCore.Logger;
using PixanKit.ResourceDownloader.Tasks.FuncTask;

namespace PixanKit.ResourceDownloader.Download.DownloadTask;

/// <summary>
/// 
/// </summary>
public class DownloadThread : FuncProgressTask<int>, IFileDownload
{
    /// <inheritdoc/>
    public long Size => _end - _start + 1;

    /// <inheritdoc/>
    public long DownloadedBytes => _downloadedBytes;

    /// <inheritdoc/>
    public int TotalFiles => 0;

    /// <inheritdoc/>
    public int DownloadedFiles => 0;

    long _start;

    long _end;

    long _downloadedBytes;

    readonly Stream _stream;

    string _url = "";

    readonly Lock _lock;

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadThread"/> class 
    /// with a specified URL, starting byte position, and ending byte position.
    /// </summary>
    /// <param name="url">The URL of the file to download.</param>
    /// <param name="stream"></param>
    /// <param name="start">The starting byte position of the chunk.</param>
    /// <param name="end">The ending byte position of the chunk.</param>
    /// <param name="locker"></param>
    public DownloadThread(string url, Stream stream, long start, long end, Lock locker) :
        this(stream, locker)
    {
        SetUrl(url, start, end);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadThread"/> class 
    /// with a specified URL, starting byte position, and ending byte position.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="locker"></param>
    public DownloadThread(Stream stream, Lock locker)
    {
        _stream = stream;
        _lock = locker;
        Function += DownloadAsync;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    public void SetUrl(string url, long start, long end)
    {
        _url = url;
        _start = start;
        _end = end;
    }


    private async Task<int> DownloadAsync(Action<double> progress, CancellationToken token)
    {
        HttpClient client = new();
        HttpResponseMessage? response = null;
        client.DefaultRequestHeaders.Range = new System.Net.Http.Headers.RangeHeaderValue(_start, _end);
        try
        {
            response = await client.GetAsync(_url, HttpCompletionOption.ResponseHeadersRead, CancellationToken.Token);
            response.EnsureSuccessStatusCode();
            await LoopRead(response.Content, progress);
        }
        catch (Exception ex)
        {
            response?.Dispose();
            Logger.Warn("PixanKit.ResourceDownloader", ex.Message);
        }
        client.Dispose();

        return 0;
    }

    private async Task LoopRead(HttpContent content, Action<double> progress)
    {
        int bytesRead;
        var stream = await content.ReadAsStreamAsync();
        var buffer = new byte[8192];
        Memory<byte> memory = new(buffer);
        while ((bytesRead = await stream.ReadAsync(memory, CancellationToken.Token)) > 0
               && !CancellationToken.IsCancellationRequested)
        {
            lock (_lock)
            {
                _stream.Position = _start + _downloadedBytes;
                _stream.Write(buffer, 0, bytesRead);
            }
            _downloadedBytes += bytesRead;
            progress((double)_downloadedBytes / Size);
        }
    }
}