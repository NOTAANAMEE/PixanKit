namespace PixanKit.ResourceDownloader.Download.DownloadTask;

/// <summary>
/// The interface defines the methods that help track the progress of the downloading task
/// </summary>
public interface IFileDownload
{
    /// <summary>
    /// Retrieves the number of bytes that has downloaded during the task
    /// </summary>
    public long DownloadedBytes { get; }

    /// <summary>
    /// Retrieves the total size that needs to download during the task
    /// </summary>
    public long Size { get; }

    /// <summary>
    /// Retrieves the number of files that has downloaded during the task
    /// </summary>
    public int DownloadedFiles { get; }

    /// <summary>
    /// Retrieves the number of files that needs to download
    /// </summary>
    public int TotalFiles { get; }
}