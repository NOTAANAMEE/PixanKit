using PixanKit.ResourceDownloader.Tasks;
using PixanKit.ResourceDownloader.Tasks.MultiTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Download
{
    /// <summary>
    /// Download File Chunk. The Result Will Be Stored In Return.<br/>
    /// Default Size Is 1 MiB
    /// </summary>
    public class ChunkDownload : ProcessTask, IFileDownloadTracer
    {
        /// <summary>
        /// Size Of A Chunk
        /// </summary>
        public static long ChunkSize = 1024 * 1024;// 1 MB

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public long Size
        { get => EndPos - StartPos + 1; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public long DownloadedChars { get => SizeDownloaded; }

        /// <summary>
        /// Return Stream
        /// </summary>
        public Stream Ret = new MemoryStream();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int FileNum { get => 1; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int DownloadedFiles { get => Size == SizeDownloaded ? 1 : 0; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Stream Return { get => Ret; set => Ret = value; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override double Schedule
        {
            get => SizeDownloaded * 10 / (double)Size;
        }

        long SizeDownloaded;

        internal long StartPos;

        long EndPos;

        string Url;

        HttpResponseMessage? response;

        HttpClient client = new();

        /// <summary>
        /// Init A ChunkDownload Task<br/>
        /// Size Of The Stream Will Be 1 MiB As Default.
        /// </summary>
        /// <param name="url">The URL Of The File</param>
        /// <param name="startPos">File Download Start Location</param>
        public ChunkDownload(string url, long startPos) : base()
        {
            StartPos = startPos;
            EndPos = startPos + ChunkSize - 1;
            Url = url;
            InitDelegate();
        }

        /// <summary>
        /// Init A ChunkDownload Task
        /// </summary>
        /// <param name="url">The URL Of The File</param>
        /// <param name="startPos">File Download Start Location</param>
        /// <param name="endPos">File Download End Location</param>
        public ChunkDownload(string url, long startPos, long endPos) : base()
        {
            StartPos = startPos;
            EndPos = endPos;
            Url = url;
            InitDelegate();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override async Task Running()
        {
            Tasks.Add(DownloadTask());
            await base.Running();
        }

        private async Task DownloadTask()
        {
            client.DefaultRequestHeaders.Range = new(StartPos, EndPos);
            response = await client.GetAsync(Url, HttpCompletionOption.ResponseHeadersRead);
            var responseStream = await response.Content.ReadAsStreamAsync();
            var buffer = new byte[8192]; // 8 KB buffer
            int bytesRead;
            while (ProcessRunning && (bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                if (_status == ProcessStatus.Canceled)
                {
                    client.Dispose();
                    return;
                }
                SizeDownloaded += bytesRead;
                Ret.Write(buffer, 0, bytesRead);
            }
            Ret.Position = 0;
        }

        private void InitDelegate()
        {
            OnCancel += CancelTask;
            OnFinish += FinishTask;
        }

        private void CancelTask()
        {
            client.Dispose();
            Ret.Dispose();
        }

        private void FinishTask()
        {
            client.Dispose();
        }
    }


}
