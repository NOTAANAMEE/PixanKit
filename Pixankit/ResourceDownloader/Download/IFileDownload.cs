using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Download
{
    /// <summary>
    /// OverAll File Download Tracer Interface
    /// </summary>
    public interface IFileDownloadTracer
    {
        /// <summary>
        /// Overall Size Of Files
        /// </summary>
        public long Size { get; }

        /// <summary>
        /// File Size That Has Downloaded
        /// </summary>
        public long DownloadedChars { get; }

        /// <summary>
        /// Overall Number Of Files
        /// </summary>
        public int FileNum { get; }

        /// <summary>
        /// Number Of Files That Has Completely Downloaded
        /// </summary>
        public int DownloadedFiles { get; }

        /// <summary>
        /// Downloaded FileStream
        /// </summary>
        public Stream Return { get; set; }
    }
}
