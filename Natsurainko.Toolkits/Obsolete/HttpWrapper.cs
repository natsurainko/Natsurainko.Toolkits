using Natsurainko.Toolkits.Network.Model;
using Natsurainko.Toolkits.Values;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Natsurainko.Toolkits.Network;

public partial class HttpWrapper
{
    /// <summary>
    /// 应该使用 
    /// <see cref="Downloader.SimpleDownloader"/>
    /// 等进行下载
    /// </summary>
    [Obsolete]
    public static async Task<HttpDownloadResponse> HttpDownloadAsync(string url, string folder, string filename = null)
    {
        FileInfo fileInfo = default;
        HttpResponseMessage responseMessage = default;

        try
        {
            responseMessage = await HttpGetAsync(url, new Dictionary<string, string>(), HttpCompletionOption.ResponseHeadersRead);
            responseMessage.EnsureSuccessStatusCode();

            if (responseMessage.Content.Headers != null && responseMessage.Content.Headers.ContentDisposition != null)
                fileInfo = new FileInfo(Path.Combine(folder, responseMessage.Content.Headers.ContentDisposition.FileName.Trim('\"')));
            else fileInfo = new FileInfo(Path.Combine(folder, Path.GetFileName(responseMessage.RequestMessage.RequestUri.AbsoluteUri)));

            if (filename != null)
                fileInfo = new FileInfo(fileInfo.FullName.Replace(fileInfo.Name, filename));

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            using var fileStream = File.Create(fileInfo.FullName);
            using var stream = await responseMessage.Content.ReadAsStreamAsync();

            byte[] bytes = new byte[BufferSize];
            int read = await stream.ReadAsync(bytes, 0, BufferSize);

            while (read > 0)
            {
                await fileStream.WriteAsync(bytes, 0, read);
                read = await stream.ReadAsync(bytes, 0, BufferSize);
            }

            fileStream.Flush();
            responseMessage.Dispose();

            GC.Collect();

            return new HttpDownloadResponse
            {
                FileInfo = fileInfo,
                HttpStatusCode = responseMessage.StatusCode,
                Message = $"{responseMessage.ReasonPhrase}[{url}]"
            };
        }
        catch (HttpRequestException e)
        {
            GC.Collect();

            return new HttpDownloadResponse
            {
                FileInfo = fileInfo,
                HttpStatusCode = (HttpStatusCode)(responseMessage?.StatusCode),
                Message = $"{e.Message}[{url}]"
            };
        }
        catch (Exception e)
        {
            return new HttpDownloadResponse
            {
                FileInfo = fileInfo,
                HttpStatusCode = HttpStatusCode.GatewayTimeout,
                Message = $"{e.Message}[{url}]"
            };
        }
    }

    /// <summary>
    /// 应该使用 
    /// <see cref="Downloader.SimpleDownloader"/>
    /// 等进行下载
    [Obsolete]
    public static async Task<HttpDownloadResponse> HttpDownloadAsync(HttpDownloadRequest request) => await HttpDownloadAsync(request.Url, request.Directory.FullName, request.FileName);

    /// <summary>
    /// 应该使用 
    /// <see cref="Downloader.SimpleDownloader"/>
    /// 等进行下载
    [Obsolete]
    public static async Task<HttpDownloadResponse> HttpDownloadAsync(string url, string folder, Action<float, string> progressChangedAction, string filename = null)
    {
        FileInfo fileInfo = default;
        HttpResponseMessage responseMessage = default;
        using var timer = new System.Timers.Timer(1000);

        try
        {
            responseMessage = await HttpGetAsync(url, new Dictionary<string, string>(), HttpCompletionOption.ResponseHeadersRead);
            responseMessage.EnsureSuccessStatusCode();

            if (responseMessage.Content.Headers != null
                && responseMessage.Content.Headers.ContentDisposition != null
                && responseMessage.Content.Headers.ContentDisposition.FileName != null)
                fileInfo = new FileInfo(Path.Combine(folder, responseMessage.Content.Headers.ContentDisposition.FileName.Trim('\"')));
            else fileInfo = new FileInfo(Path.Combine(folder, Path.GetFileName(responseMessage.RequestMessage.RequestUri.AbsoluteUri)));

            if (filename != null)
                fileInfo = new FileInfo(fileInfo.FullName.Replace(fileInfo.Name, filename));

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            using var fileStream = File.Create(fileInfo.FullName);
            using var stream = await responseMessage.Content.ReadAsStreamAsync();

            timer.Elapsed += delegate { progressChangedAction.Invoke(fileStream.Length / (float)responseMessage.Content.Headers.ContentLength, $"{fileStream.Length.LengthToMb()} / {((long)responseMessage.Content.Headers.ContentLength).LengthToMb()}"); };
            timer.Start();

            byte[] bytes = new byte[BufferSize];
            int read = await stream.ReadAsync(bytes, 0, BufferSize);

            while (read > 0)
            {
                await fileStream.WriteAsync(bytes, 0, read);
                read = await stream.ReadAsync(bytes, 0, BufferSize);
            }

            fileStream.Flush();
            responseMessage.Dispose();

            timer.Stop();

            GC.Collect();

            return new HttpDownloadResponse
            {
                FileInfo = fileInfo,
                HttpStatusCode = responseMessage.StatusCode,
                Message = $"{responseMessage.ReasonPhrase}[{url}]"
            };
        }
        catch (HttpRequestException e)
        {
            if (timer.Enabled)
                timer.Stop();

            GC.Collect();

            return new HttpDownloadResponse
            {
                FileInfo = fileInfo,
                HttpStatusCode = (HttpStatusCode)(responseMessage?.StatusCode),
                Message = $"{e.Message}[{url}]"
            };
        }
        catch (Exception e)
        {
            if (timer.Enabled)
                timer.Stop();

            GC.Collect();

            return new HttpDownloadResponse
            {
                FileInfo = fileInfo,
                HttpStatusCode = HttpStatusCode.GatewayTimeout,
                Message = $"{e.Message}[{url}]"
            };
        }
    }

    /// <summary>
    /// 应该使用 
    /// <see cref="Downloader.SimpleDownloader"/>
    /// 等进行下载
    [Obsolete]
    public static async Task<HttpDownloadResponse> HttpDownloadAsync(HttpDownloadRequest request, Action<float, string> progressChangedAction) => await HttpDownloadAsync(request.Url, request.Directory.FullName, progressChangedAction, request.FileName);
}

