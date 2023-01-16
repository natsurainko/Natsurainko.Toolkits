using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Natsurainko.Toolkits.Network.Downloader;

public class DownloadRequest
{
    public DirectoryInfo Directory { get; set; }

    public string Url { get; set; }

    public string FileName { get; set; }

    public long? FileSize { get; set; }
}

public enum DownloaderCompletionType
{
    AllCompleted = 0,
    PartiallyCompleted = 1,
    Uncompleted = 2
}

public interface IDownloaderProgressChangedEventArgs
{
    public double Progress { get; }
}

public class DownloadResponse
{
    public HttpStatusCode HttpStatusCode { get; set; }

    public DownloaderCompletionType CompletionType { get; set; }

    public TimeSpan DownloadTime { get; set; }

    public Exception Exception { get; set; }
}

public class DownloaderResponse<TResult> : DownloadResponse
{
    public TResult Result { get; set; }
}

public class SimpleDownloaderProgressChangedEventArgs : IDownloaderProgressChangedEventArgs
{
    public double Progress => CompletedLength / (double)TotleLength;

    public long TotleLength { get; set; }

    public long CompletedLength { get; set; }
}

public class SimpleDownloaderResponse : DownloaderResponse<FileInfo>
{
    public bool Success { get; set; }
}

public class ParallelDownloaderProgressChangedEventArgs : IDownloaderProgressChangedEventArgs
{
    public double Progress => CompletedTasks / (double)TotleTasks;

    public int TotleTasks { get; set; }

    public int CompletedTasks { get; set; }
}

public class ParallelDownloaderResponse : DownloadResponse
{
    public int TotleTasks { get; set; }

    public int CompletedTasks { get; set; }

    public List<DownloadRequest> FailedRequests { get; set; }
}
