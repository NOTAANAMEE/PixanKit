using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixanKit.ResourceDownloader.Tasks;
using PixanKit.ResourceDownloader.Tasks.MultiProgressTask;

namespace PixanKit.ResourceDownloader.Download.DownloadTask
{
    public class FileDownloadTask:AsyncProgressTask
    {
        public static int ThreadNum = 64;

        private string _url;

        private string _fileName;

        private FileStream _stream;

        private object _lock = new();

        private int threadnum = 1;

        public FileDownloadTask(string url, string path):this(url, path, ThreadNum)
        {    }

        public FileDownloadTask(string url, string path, int threanum)
        {
            this.threadnum = threanum;
            _url = url;
            _fileName = path;
            _stream = new FileStream(path, FileMode.Create);
            OnFinish += (a) =>
            {
                _stream.Close();
            };
            Init();
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

        private void ChunkReturn(ProgressTask task)
        {
            FileChunkDownloadTask downloadTask = (FileChunkDownloadTask)task;

            lock (_lock) 
            {
                _stream.Position = downloadTask._start;
                downloadTask.Return.CopyTo(_stream);
                _stream.Flush();
            }

            downloadTask.Return.DisposeAsync();
        }
    }
}
