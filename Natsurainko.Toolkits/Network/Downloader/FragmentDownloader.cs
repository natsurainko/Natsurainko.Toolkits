using Natsurainko.Toolkits.Values;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Natsurainko.Toolkits.Network.Downloader;

public class FragmentDownloader
    : DownloaderBase<SimpleDownloaderResponse, SimpleDownloaderProgressChangedEventArgs>
{
    public FragmentDownloader(DownloadRequest downloadRequest, int partCount = 8)
    {
        this.Url = downloadRequest.Url;
        this.PartCount = partCount;
        this.DownloadFolder = downloadRequest.Directory.FullName;
        this.TargetFileName = downloadRequest.FileName;
    }

    public FragmentDownloader(string url, string downloadFolder, string targetFileName = null, int partCount = 8)
    {
        this.Url = url;
        this.PartCount = partCount;
        this.DownloadFolder = downloadFolder;
        this.TargetFileName = targetFileName;
    }

    public long TotleLength { get; private set; }

    public long CompletedLength { get; private set; }

    public string Url { get; private set; }

    public string TargetFileName { get; private set; }

    public string DownloadFolder { get; private set; }

    public int PartCount { get; private set; }

    public HttpResponseMessage HttpResponseMessage { get; private set; }

    public override void BeginDownload()
    {
        base.BeginDownload();

        DownloadProcess = Task.Run(async () =>
        {
            using var responseMessage = await HttpWrapper.HttpGetAsync
                (Url, new Dictionary<string, string>() { { "Range", "bytes=0-1" } }, HttpCompletionOption.ResponseHeadersRead);
            responseMessage.EnsureSuccessStatusCode();

            HttpResponseMessage = await HttpWrapper.HttpGetAsync(Url, new Dictionary<string, string>(), HttpCompletionOption.ResponseHeadersRead);
            HttpResponseMessage.EnsureSuccessStatusCode();

            if (!Directory.Exists(DownloadFolder))
                Directory.CreateDirectory(DownloadFolder);

            TotleLength = (long)HttpResponseMessage.Content.Headers.ContentLength;
            if (HttpResponseMessage.Content.Headers.ContentDisposition != null && !string.IsNullOrEmpty(HttpResponseMessage.Content.Headers.ContentDisposition.FileName) && string.IsNullOrEmpty(TargetFileName))
                TargetFileName = HttpResponseMessage.Content.Headers.ContentDisposition.FileName.Trim('\"');

            if (string.IsNullOrEmpty(TargetFileName))
                TargetFileName = Path.GetFileName(HttpResponseMessage.RequestMessage.RequestUri.AbsoluteUri);

            var downloaders = (TotleLength + 1).SplitIntoRange(PartCount).Select(x => new SimpleDownloader
                    (Url, DownloadFolder, $"{TargetFileName}_Part_{x.Item1}_To_{x.Item2 - 1}", x.Item1, x.Item2 - 1)).ToList();

            downloaders.ForEach(x => x.DownloadProgressChanged += (_, _) => Task.Run(() =>
            {
                long cache = 0;
                downloaders.Select(x => x.CompletedLength).ToList().ForEach(x => cache += x);

                CompletedLength = cache;
                OnProgressChanged(new SimpleDownloaderProgressChangedEventArgs
                {
                    TotleLength = TotleLength,
                    CompletedLength = CompletedLength
                });
            }));

            downloaders.ForEach(x => x.BeginDownload());
            var downloaderResponses = await Task.WhenAll(downloaders.Select(async x => await x.CompleteAsync()));

            foreach (var item in downloaders)
                item.Dispose();

            using var fileStream = File.Create(Path.Combine(DownloadFolder, TargetFileName));

            foreach (var item in downloaderResponses)
            {
                if (item.Success)
                {
                    var buffer = File.ReadAllBytes(item.Result.FullName);
                    await fileStream.WriteAsync(buffer, 0, buffer.Length);

                    File.Delete(item.Result.FullName);
                }
            }

            await fileStream.FlushAsync();

            if (fileStream.Length != TotleLength)
                throw new Exception("Download Failed");

        }).ContinueWith(task =>
        {
            DownloadTimeStopwatch.Stop();

            if (task.IsFaulted)
                OnDownloadFailed(task.Exception);

            var simpleDownloaderResponse = new SimpleDownloaderResponse
            {
                Exception = task.Exception,
                Success = !task.IsFaulted,
                CompletionType = task.IsFaulted ? DownloaderCompletionType.Uncompleted : DownloaderCompletionType.AllCompleted,
                DownloadTime = DownloadTimeStopwatch.Elapsed,
                Result = new FileInfo(Path.Combine(DownloadFolder, TargetFileName))
            };

            OnDownloadCompleted(simpleDownloaderResponse);
            return simpleDownloaderResponse;
        });
    }

    public override void Dispose()
    {
        HttpResponseMessage.Dispose();
        DownloadProcess.Dispose();

        HttpResponseMessage = null;
        DownloadProcess = null;

        GC.Collect();
    }
}
