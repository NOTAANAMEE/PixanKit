using PixanKit.ResourceDownloader.Tasks;
using PixanKit.ResourceDownloader.Tasks.MultiProgressTask;

namespace PixanKit.ResourceDownloader.Download.DownloadTask;

/// <summary>
/// Represents a task for downloading multiple files concurrently using multiple threads.
/// </summary>
public class MultiFileDownloadTask : AsyncProgressTask, IFileDownload
{
    /// <summary>
    /// The default number of threads to use for downloading.
    /// </summary>
    public static int ThreadNum = 64;

    /// <summary>
    /// The URLs of the files to download.
    /// </summary>
    protected string[] Urls = [];

    /// <summary>
    /// The file paths where the downloaded files will be saved.
    /// </summary>
    protected string[] Paths = [];

    /// <summary>
    /// The number of threads to use for downloading.
    /// </summary>
    protected int Threadnum = 1;

    List<FileDownloadTask> _files = [];

    /// <inheritdoc/>
    public long Size
    {
        get
        {
            return _files.Sum(task => task.Size);
        }
    }

    /// <inheritdoc/>
    public long DownloadedBytes
    {
        get
        {
            return _files.Sum(task => task.DownloadedBytes);
        }
    }

    /// <inheritdoc/>
    public int TotalFiles => Paths.Length;

    /// <inheritdoc/>
    public int DownloadedFiles  => _files.Sum(task => task.DownloadedFiles);

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiFileDownloadTask"/> class 
    /// with specified URLs and file paths, using the default number of threads.
    /// </summary>
    /// <param name="url">An array of file URLs to download.</param>
    /// <param name="path">An array of file paths where the files will be saved.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the length of <paramref name="url"/> and <paramref name="path"/> are not equal.
    /// </exception>
    public MultiFileDownloadTask(string[] url, string[] path) : this(url, path, ThreadNum)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiFileDownloadTask"/> class 
    /// with specified URLs, file paths, and the number of threads to use.
    /// </summary>
    /// <param name="url">An array of file URLs to download.</param>
    /// <param name="path">An array of file paths where the files will be saved.</param>
    /// <param name="threanum">The number of threads to use for downloading.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the length of <paramref name="url"/> and <paramref name="path"/> are not equal.
    /// </exception>
    public MultiFileDownloadTask(string[] url, string[] path, int threanum)
    {
        if (url.Length != path.Length)
            throw new InvalidOperationException("url Should Contain Same Amount Of path");
        this.Threadnum = threanum;
        Urls = url;
        Paths = path;
        Init();
    }

    internal MultiFileDownloadTask() { }

    internal void Set(string[] url, string[] path)
    {
        Urls = url;
        Paths = path;
        Init();
    }

    /// <summary>
    /// Initializes the download tasks by dividing the files across multiple threads.
    /// </summary>
    /// <remarks>
    /// Each file is assigned to a sequence task, ensuring that downloads are grouped based on the available threads.
    /// If more files are present than threads, files are distributed cyclically.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if there is a mismatch between the number of URLs and paths.
    /// </exception>
    protected void Init()
    {
        var count = 0;

        List<SequenceProgressTask> tasks = [];

        for (var i = 0; i < Urls.Length; i++)
        {
            //If ProgressTasks Does Not Have So Many Tasks, Add A New Task
            if (ProgressTasks.Count < count + 1) tasks.Add(new SequenceProgressTask());
            var task = new FileDownloadTask(Urls[i], Paths[i], 1);
            _files.Add(task);
            tasks[count].Add(task);
            count++;
            if (count == Threadnum) count = 0;
        }
        ProgressTasks = tasks.Cast<ProgressTask>().ToList();
    }
}