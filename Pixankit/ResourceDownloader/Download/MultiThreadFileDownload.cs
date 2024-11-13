using PixanKit.ResourceDownloader.SystemInf;
using PixanKit.ResourceDownloader.Tasks.MultiTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Download
{
    /// <summary>
    /// Multithreading Download File
    /// </summary>
    public class MultiThreadDownload : MultiAsyncTask, IFileDownloadTracer
    {
        /// <summary>
        /// Num Of Thread
        /// </summary>
        public static int ThreadCount = 64;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public long Size { get => length; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public long DownloadedChars
        {
            get
            {
                long count = 0;
                foreach (var chunk in fileChunks)
                {
                    count += chunk.DownloadedChars;
                }
                return count;
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
        public Stream Return { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override double Schedule
        {
            get => downloadchars / (double)Size;
        }

        int threadnum = ThreadCount;

        string url;

        string path;

        List<ChunkDownload> fileChunks = new();

        long length;

        long downloadchars;

        object Lock = new();

        /// <summary>
        /// Init A MultiThreadDownload Task Instance
        /// </summary>
        /// <param name="URL">The URL Of The File</param>
        /// <param name="Path">The Destination</param>
        public MultiThreadDownload(string URL, string Path) : this(Path)
        {
            SetURL(URL);
        }

        /// <summary>
        /// Initor
        /// </summary>
        /// <param name="Path"></param>
        public MultiThreadDownload(string Path)
        {
            path = Path;
            Directory.CreateDirectory(Localize.GetLocalDirectory(Path));
            Return = new FileStream(Localize.PathLocalize(Path), FileMode.Create);
            OnFinish += FinishTask;
            OnCancel += CancelTask;
        }

        /// <summary>
        /// Set URL
        /// </summary>
        /// <param name="URL"></param>
        public void SetURL(string URL) 
        {
            url = URL;
            Init().Wait();
        }

        private async Task Init()
        {
            HttpClient client = new();
            var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            client.Dispose();
            length = response.Content.Headers.ContentLength ?? 0;
            long tmp = length, chunksize = ChunkDownload.ChunkSize;
            int count = 0;
            while (tmp > 0)
            {
                if (processes.Count < count)
                    processes.Add(new MultiSequenceTask());
                tmp -= chunksize;
                ChunkDownload task;
                if (tmp >= 0)
                    task = new ChunkDownload(url, tmp);
                else
                    task = new ChunkDownload(url, 0, tmp + chunksize);
                task.OnFinish += () =>
                {
                    lock (Lock)
                    {
                        Return.Position = task.StartPos;
                        task.Ret.CopyTo(Return);
                        Return.Flush();
                    }
                };
                count++;
                if (count == threadnum) count = 0;
            }
        }

        private void FinishTask()
        {
            Return.Close();
        }

        private void CancelTask()
        {
            Return.Close();
            File.Delete(path);
        }
    }
}
