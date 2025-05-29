using PixanKit.ResourceDownloader.Tasks;
using PixanKit.ResourceDownloader.Tasks.FuncTask;
using PixanKit.ResourceDownloader.Tasks.MultiProgressTask;

namespace PixanKit.ResourceDownloader.Download.DownloadTask;

/// <summary>
/// Represents a task for downloading a file from a given URL using multiple threads.
/// </summary>
public class FileDownloadTask : SequenceProgressTask, IFileDownload
{
    /// <summary>
    /// The default number of threads to use for downloading.
    /// </summary>
    public static int ThreadNum = 64;

    /// <inheritdoc/>
    public long Size => _size;

    /// <inheritdoc/>
    public long DownloadedBytes
    {
        get
        {
            long ret = 0;
            foreach (var item in ProgressTasks)
            {
                ret += ((FileChunkDownloadTask)item).DownloadedBytes;
            }
            return ret;
        }
    }

    /// <inheritdoc/>
    public int TotalFiles => 1;

    /// <inheritdoc/>
    public int DownloadedFiles => (this.Status == ProgressStatus.Finished) ? 1 : 0;

    private string _url;

    private readonly string _savePath;

    private readonly FileStream _stream;

    private readonly Lock _fileLock = new();

    private readonly int _threadNum;

    private long _size;

    private readonly FuncProgressTask<int> _initTask = new();

    private readonly AsyncProgressTask<DownloadThread> _downloadTask = new();


    /// <summary>
    /// Initializes a new instance of the <see cref="FileDownloadTask"/> class 
    /// with a specified URL, file path, and the default number of threads.
    /// </summary>
    /// <param name="url">The URL of the file to download.</param>
    /// <param name="path">The path where the file will be saved.</param>
    public FileDownloadTask(string url, string path) : this(url, path, ThreadNum)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileDownloadTask"/> class 
    /// with a specified URL, file path, and the number of threads to use.
    /// </summary>
    /// <param name="url">The URL of the file to download.</param>
    /// <param name="path">The path where the file will be saved.</param>
    /// <param name="threadNum">The number of threads to use for downloading.</param>
    public FileDownloadTask(string url, string path, int threadNum)
    {
        this._threadNum = threadNum;
        _url = url;
        _savePath = path;
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "./");
        _stream = new FileStream(path, FileMode.Create);
        OnCancel += CancelRun;
        OnFinish += _ =>
        {
            _stream.Close();
        };
        Init();
    }

    private void Init()
    {
        Add(_initTask);
        Add(_downloadTask);
        _initTask.Function += InitRun;
        for (var i = 0; i < _threadNum; i++)
        {
            _downloadTask.Add(new DownloadThread(_stream, _fileLock));
        }
    }

    private async Task<int> InitRun(Action<double> report, CancellationToken token)
    {
        HttpClient client = new();
        long threadCounter = 0;
        var response = await client.GetAsync(_url,
            HttpCompletionOption.ResponseHeadersRead, token);
        response.EnsureSuccessStatusCode();
        var length = response.Content.Headers.ContentLength ?? 0;
        var threadSize = length / _threadNum;
        var mod = length % _threadNum;
        _size = length;
        response.Dispose();
        client.Dispose();
        foreach (var thread in _downloadTask.ProgressTasks)
        {
            if (token.IsCancellationRequested) throw new TaskCanceledException();
            var end = threadCounter + threadSize + ((--mod >= 0) ? 1 : 0);
            thread.SetUrl(_url, threadCounter, end - 1);
            threadCounter = end;
        }
        return 0;
    }

    /// <summary>
    /// Start The Task<br/>
    /// It will first check whether the url is not blank, then start the task.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public override void Start()
    {
        if (_url == "") throw new InvalidOperationException("Set A URL");
        base.Start();
    }

    internal void SetUrl(string url)
    {
        _url = url;
    }



    private void CancelRun(ProgressTask t)
    {
        _stream.Close();
        File.Delete(_savePath);
    }
}