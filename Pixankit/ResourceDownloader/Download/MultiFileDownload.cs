using PixanKit.ResourceDownloader.Tasks.MultiTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Download
{
    /// <summary>
    /// Download Multiple Files Uisng Multiple Threads
    /// </summary>
    public class MultiFileDownload : MultiAsyncTask, IFileDownloadTracer
    {
        /// <summary>
        /// Num Of Thread
        /// </summary>
        public static int ThreadCount = 64;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public long Size { get => 0; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public long DownloadedChars
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int FileNum { get => 1; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int DownloadedFiles { get => DownloadedChars == Size ? 1 : 0; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Stream Return { get; set; } = new MemoryStream();

        int ThreadNum = ThreadCount;

        List<MultiThreadDownload> files = new();

        int current = 0;

        /// <summary>
        /// Init A MultiFileDownload Task With Overall ThreadCount Setting
        /// </summary>
        /// <param name="urls"></param>
        /// <param name="paths"></param>
        /// <exception cref="ArgumentException"></exception>
        public MultiFileDownload(string[] urls, string[] paths) : this(urls, paths, ThreadCount)
        {

        }

        /// <summary>
        /// Init A MultiFileDownload Task With threadnum To Set The Number Of Threads
        /// </summary>
        /// <param name="urls"></param>
        /// <param name="paths"></param>
        /// <param name="threadnum"></param>
        /// <exception cref="ArgumentException"></exception>
        public MultiFileDownload(string[] urls, string[] paths, int threadnum) : this(threadnum)
        {
            if (urls.Length != paths.Length) throw new ArgumentException();
            for (int i = 0; i < urls.Length; i++) Add(urls[i], paths[i]);
        }

        /// <summary>
        /// Init A MultFileDownload Without A 
        /// </summary>
        /// <param name="threadnum"></param>
        public MultiFileDownload(int threadnum) : base()
        {
            ThreadNum = threadnum;
        }

        public MultiFileDownload() : this(ThreadCount) { }

        /// <summary>
        /// Add A File For Download
        /// </summary>
        /// <param name="url">The URL Of The File</param>
        /// <param name="path">The Direction Of The File</param>
        public void Add(string url, string path)
        {
            var mul = new MultiThreadDownload(url, path);
            files.Add(mul);
            AddTask(mul);
        }

        private void AddTask(MultiThreadDownload task)
        {
            if (processes.Count == current % ThreadNum - 1) processes.Add(new MultiSequenceTask());
            (processes[current % ThreadNum] as MultiSequenceTask).Add(task);
        }
    }
}
