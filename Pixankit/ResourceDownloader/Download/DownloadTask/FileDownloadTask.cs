using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixanKit.ResourceDownloader.SystemInf;
using PixanKit.ResourceDownloader.Tasks;
using PixanKit.ResourceDownloader.Tasks.MultiProgressTask;

namespace PixanKit.ResourceDownloader.Download.DownloadTask
{
    /// <summary>
    /// Represents a task for downloading a file from a given URL using multiple threads.
    /// </summary>
    public class FileDownloadTask:AsyncProgressTask
    {
        /// <summary>
        /// The default number of threads to use for downloading.
        /// </summary>
        public static int ThreadNum = 64;

        private string _url;

        private string _fileName;

        private FileStream _stream;

        private object _filelock = new();

        private int threadnum = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileDownloadTask"/> class 
        /// with a specified URL, file path, and the default number of threads.
        /// </summary>
        /// <param name="url">The URL of the file to download.</param>
        /// <param name="path">The path where the file will be saved.</param>
        public FileDownloadTask(string url, string path):this(url, path, ThreadNum)
        {    }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileDownloadTask"/> class 
        /// with a specified URL, file path, and the number of threads to use.
        /// </summary>
        /// <param name="url">The URL of the file to download.</param>
        /// <param name="path">The path where the file will be saved.</param>
        /// <param name="threadnum">The number of threads to use for downloading.</param>
        public FileDownloadTask(string url, string path, int threadnum)
        {
            this.threadnum = threadnum;
            _url = url;
            _fileName = path;
            _stream = new FileStream(path, FileMode.Create);
            OnCancel += ChunkReturn;
            OnFinish += (a) =>
            {
                _stream.Close();
            };
            Init();
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

        internal void SetURL(string url)
        {
            _url = url;
            Init();
        }

        private void Init()
        {
            if (_url == "") return;
            HttpClient client = new();
            var request = new HttpRequestMessage(HttpMethod.Get, _url);
            var response = client.Send(request, HttpCompletionOption.ResponseHeadersRead);
            long length = response.Content.Headers.ContentLength ?? 0;
            int count = 0;

            while (length > 0) 
            {
                ProgressTask task;

                //If ProgressTasks Does Not Have So Many Tasks, Add A New Task
                if (ProgressTasks.Count < count + 1) Add(new SequenceProgressTask());

                length -= FileChunkDownloadTask.ChunkSize;

                if (length < 0)
                    task = new FileChunkDownloadTask(_url, 
                        0, length + FileChunkDownloadTask.ChunkSize - 1);
                else 
                    task = new FileChunkDownloadTask(_url, 
                        length);

                (ProgressTasks[count] as SequenceProgressTask).
                    Add(task);
                task.OnFinish += ChunkReturn;
                count++;
                if (count == threadnum) count = 0;
            }

            client.Dispose();
            request.Dispose();
            response.Dispose();
        }

        private async void ChunkReturn(ProgressTask task)
        {
            FileChunkDownloadTask downloadTask = (FileChunkDownloadTask)task;

            lock (_filelock) 
            {
                _stream.Position = downloadTask._start;
                downloadTask.Return.CopyTo(_stream);
                _stream.Flush();
            }

            await downloadTask.Return.DisposeAsync().AsTask();
        }

        private void CancelRun(ProgressTask t)
        {
            _stream.Dispose();
            File.Delete(_fileName);
        }
    }
}
