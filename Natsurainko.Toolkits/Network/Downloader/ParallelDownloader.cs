using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Natsurainko.Toolkits.Network.Downloader;

public class ParallelDownloader<TSource>
    : DownloaderBase<ParallelDownloaderResponse, ParallelDownloaderProgressChangedEventArgs>
{
    public static long LargeFileLength { get; set; } = 1572864L;

    public ParallelDownloader(
        IEnumerable<TSource> source,
        Func<TSource, DownloadRequest> func,
        bool enableLargeFileFragment = true,
        int parallelBufferCapacity = 128)
    {
        Func = func;
        Source = source;
        EnableLargeFileFragment = enableLargeFileFragment;
        ParallelBufferCapacity = parallelBufferCapacity;
    }

    public Func<TSource, DownloadRequest> Func { get; private set; }

    public IEnumerable<TSource> Source { get; private set; }

    public List<DownloadRequest> FailedRequests { get; private set; } = new();

    public int CompletedTasks { get; private set; }

    public int TotleTasks { get; private set; }

    public bool EnableLargeFileFragment { get; set; }

    public int ParallelBufferCapacity { get; set; }

    public override void BeginDownload()
    {
        base.BeginDownload();

        DownloadProcess = Task.Run(async () =>
        {
            TotleTasks = Source.Count();

            var manyBlock = new TransformManyBlock<IEnumerable<TSource>, DownloadRequest>(x => x.Select(x => Func(x)));
            var actionBlock = new ActionBlock<DownloadRequest>(async request =>
            {
                using IDownloader<SimpleDownloaderResponse, SimpleDownloaderProgressChangedEventArgs> downloader
                    = EnableLargeFileFragment && request.FileSize.GetValueOrDefault(0L) >= LargeFileLength
                    ? new FragmentDownloader(request)
                    : new SimpleDownloader(request);

                downloader.BeginDownload();

                var response = await downloader.CompleteAsync();

                if (response.Success)
                    CompletedTasks++;
                else FailedRequests.Add(request);

                OnProgressChanged(new ParallelDownloaderProgressChangedEventArgs
                {
                    CompletedTasks = CompletedTasks,
                    TotleTasks = TotleTasks
                });
            }, new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = ParallelBufferCapacity,
                MaxDegreeOfParallelism = ParallelBufferCapacity
            });

            using var disposable = manyBlock.LinkTo(actionBlock, new DataflowLinkOptions { PropagateCompletion = true });

            manyBlock.Post(Source);
            manyBlock.Complete();

            await actionBlock.Completion;
        }).ContinueWith(task =>
        {
            DownloadTimeStopwatch.Stop();

            if (task.IsFaulted)
                OnDownloadFailed(task.Exception);

            var parallelDownloaderResponse = new ParallelDownloaderResponse
            {
                Exception = task.Exception,
                DownloadTime = DownloadTimeStopwatch.Elapsed,
                CompletedTasks = CompletedTasks,
                TotleTasks = TotleTasks,
                FailedRequests = FailedRequests,
                CompletionType = CompletedTasks == 0
                    ? DownloaderCompletionType.Uncompleted :
                        CompletedTasks == TotleTasks ?
                            DownloaderCompletionType.AllCompleted : DownloaderCompletionType.PartiallyCompleted
            };

            OnDownloadCompleted(parallelDownloaderResponse);
            return parallelDownloaderResponse;
        });
    }

    public override void Dispose()
    {
        DownloadProcess.Dispose();

        Func = null;
        Source = null;
        FailedRequests = null;
        DownloadProcess = null;
    }
}
