using System;
using System.IO;
using System.Net;

namespace Natsurainko.Toolkits.Network.Model;

/// <summary>
/// 应该使用 
/// <see cref="Downloader.DownloadRequest"/>
/// 来定义下载请求
/// </summary>
[Obsolete]
public class HttpDownloadRequest
{
    public DirectoryInfo Directory { get; set; }

    public string Url { get; set; }

    public int? Size { get; set; }

    public string Sha1 { get; set; }

    public string FileName { get; set; }
}

/// <summary>
/// 应该使用 
/// <see cref="Downloader.DownloadResponse"/>
/// 及其泛型来定义下载返回
/// </summary>
[Obsolete]
public class HttpDownloadResponse
{
    public string Message { get; set; }

    public HttpStatusCode HttpStatusCode { get; set; }

    public FileInfo FileInfo { get; set; }
}
