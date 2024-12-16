using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceDownloader.Download.DownloadTask
{
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
}
