using PixanKit.ResourceDownloader.Tasks.FuncTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Download.DownloadTask
{
    /// <summary>
    /// Represents a task for downloading a specific chunk of a file from a given URL.
    /// </summary>
    public class FileChunkDownloadTask : FuncProgressTask<Stream>
    {
        /// <summary>
        /// The default size of a file chunk in bytes.
        /// </summary>
        public static readonly long ChunkSize = 1024 * 1024;

        private readonly string _url;

        /// <summary>
        /// The starting byte position of the file chunk to download.
        /// </summary>
        public readonly long _start;

        /// <summary>
        /// The ending byte position of the file chunk to download.
        /// </summary>
        public readonly long _end;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileChunkDownloadTask"/> class 
        /// with a specified URL, starting byte position, and ending byte position.
        /// </summary>
        /// <param name="url">The URL of the file to download.</param>
        /// <param name="start">The starting byte position of the chunk.</param>
        /// <param name="end">The ending byte position of the chunk.</param>
        public FileChunkDownloadTask(string url, long start, long end): base()
        {
            _url = url;
            _start = start;
            _end = end;
            Function += DownloadAsync;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileChunkDownloadTask"/> class 
        /// with a specified URL and starting byte position. The chunk size is set to the default value.
        /// </summary>
        /// <param name="url">The URL of the file to download.</param>
        /// <param name="start">The starting byte position of the chunk.</param>
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
