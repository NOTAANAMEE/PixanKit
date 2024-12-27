using PixanKit.ResourceDownloader.Tasks.FuncTask;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceDownloader.Download.DownloadTask
{
    public class SimpleFileDownloadTask : FuncProgressTask<int>, IFileDownload
    {
        public long DownloadedBytes => throw new NotImplementedException();

        public long Size => throw new NotImplementedException();

        public int DownloadedFiles => throw new NotImplementedException();

        public int TotalFiles => throw new NotImplementedException();

        string _url = "";

        FileStream _stream;

        string _path;

        public SimpleFileDownloadTask(string url, string path)
        {
            Function += DownloadAsync;
            _url = url;
            _path = path;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            _stream = new FileStream(path, FileMode.Create);
            OnCancel += (a) =>
            {
                _stream.Dispose();
                File.Delete(path);
            };
            OnFinish += (a) =>
            {
                _stream.Close();
            };
        }

        private async Task<int> DownloadAsync(Action<double> progress, CancellationToken token)
        {
            HttpClient client = new();
            var bytes = new byte[8192];
            int bytesRead;
            long downloadedBytes = 0;
            try
            {
                using var response = await client.GetAsync(_url, HttpCompletionOption.ResponseHeadersRead, token);
                response.EnsureSuccessStatusCode();
                using var stream = await response.Content.ReadAsStreamAsync(token);

                while ((bytesRead = await stream.ReadAsync(bytes, 0, bytes.Length, token)) > 0)
                {
                    Console.WriteLine($"{_stream.Length} - {_stream.Length + bytesRead - 1}");
                    await _stream.WriteAsync(bytes, 0, bytesRead, token);
                    downloadedBytes += bytesRead;
                }
            }
            catch (OperationCanceledException)
            {
                progress(0);
            }
            finally
            {
                client.Dispose();
            }

            _stream.Position = 0;
            return 0;
        }

        internal void SetURL(string url)
        {
            _url = url;
        }
    }
}
