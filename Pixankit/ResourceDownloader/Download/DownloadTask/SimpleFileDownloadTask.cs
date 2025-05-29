using PixanKit.ResourceDownloader.Tasks.FuncTask;

namespace PixanKit.ResourceDownloader.Download.DownloadTask;

/// <summary>
/// The class represents a task to download a file which is small 
/// or the resource that the size is not being sent by the server
/// </summary>
public class SimpleFileDownloadTask : FuncProgressTask<int>, IFileDownload
{
    /// <inheritdoc/>
    public long DownloadedBytes => _downloadedBytes;

    /// <inheritdoc/>
    public long Size => 0;

    /// <inheritdoc/>
    public int DownloadedFiles => 0;

    /// <inheritdoc/>
    public int TotalFiles => 1;

    string _url;

    readonly FileStream _stream;

    long _downloadedBytes;

    /// <summary>
    /// Inits the download task with certain url and path
    /// </summary>
    /// <param name="url">the url of the resource</param>
    /// <param name="path">the target path</param>
    public SimpleFileDownloadTask(string url, string path)
    {
        Function += DownloadAsync;
        _url = url;
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "");
        _stream = new FileStream(path, FileMode.Create);
        OnCancel += _ =>
        {
            _stream.Dispose();
            File.Delete(path);
        };
        OnFinish += _ =>
        {
            _stream.Close();
        };
    }

    private async Task<int> DownloadAsync(Action<double> progress, CancellationToken token)
    {
        HttpClient client = new();
        var bytes = new byte[8192];

        try
        {
            using var response = await client.GetAsync(_url, HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(token);

            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(bytes, 0, bytes.Length, token)) > 0)
            {
                //Console.WriteLine($"{_stream.Length} - {_stream.Length + bytesRead - 1}");
                await _stream.WriteAsync(bytes, 0, bytesRead, token);
                _downloadedBytes += bytesRead;
            }
        }
        catch (OperationCanceledException)
        {
            progress(0);
        }
        finally
        {
            client.Dispose();
        }

        _stream.Position = 0;
        return 0;
    }

    internal void SetUrl(string url)
    {
        _url = url;
    }
}