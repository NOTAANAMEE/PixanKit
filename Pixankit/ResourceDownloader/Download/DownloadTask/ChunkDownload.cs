using PixanKit.ResourceDownloader.Tasks.FuncTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Download.DownloadTask
{
    public class FileChunkDownloadTask : FuncProgressTask<Stream>
    {
        public static readonly long ChunkSize = 1024 * 1024;

        private readonly string _url;

        public readonly long _start;

        public readonly long _end;

        public FileChunkDownloadTask(string url, long start, long end): base()
        {
            _url = url;
            _start = start;
            _end = end;
            Function += DownloadAsync;
        }

        public FileChunkDownloadTask(string url, long start): base()
        {
            _url = url;
            _start = start;
            _end = start + ChunkSize - 1;
            Function += DownloadAsync;
        }

        private async Task<Stream> DownloadAsync(Action<double> progress, CancellationToken token)
        {
            HttpClient client = new();
            var buffer = new byte[8192];
            int bytesRead;
            long downloadedBytes = 0;
            var totalBytes = _end - _start + 1;
            var request = new HttpRequestMessage(HttpMethod.Get, _url);
            Stream stream;
            MemoryStream ret = new();
            HttpResponseMessage response;

                request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(_start, _end);
                response = client.Send(request, HttpCompletionOption.ResponseHeadersRead, CancellationToken.Token);
                response.EnsureSuccessStatusCode();
                stream = await response.Content.ReadAsStreamAsync(token);

                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0 && !CancellationToken.IsCancellationRequested)
                {
                        ret.Write(buffer, 0, bytesRead);


                    downloadedBytes += bytesRead;    
                    progress((double)downloadedBytes / totalBytes);
                }
                if (!token.IsCancellationRequested) progress(1.0);
                client.Dispose();
                response.Dispose();
                request.Dispose();
            return ret;
        }
    }

}
