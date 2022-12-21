using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Stream = System.IO.Stream;

namespace Natsurainko.Toolkits.Network;

public partial class HttpWrapper
{
    public static int BufferSize { get; set; } = 1024 * 1024;

    public static readonly HttpClient HttpClient = new() { Timeout = new TimeSpan(TimeSpan.TicksPerSecond * 30) };

    public static async Task<bool> VerifyHttpConnect(string url)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Head, url);

        var res = await HttpClient.SendAsync(requestMessage);
        var ret = res.IsSuccessStatusCode;

        res.Dispose();
        requestMessage.Dispose();

        return ret;
    }

    public static async Task<HttpResponseMessage> HttpGetAsync(string url, Tuple<string, string> authorization = default, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

        if (authorization != null)
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue(authorization.Item1, authorization.Item2);

        var responseMessage = await HttpClient.SendAsync(requestMessage, httpCompletionOption, CancellationToken.None);

        if (responseMessage.StatusCode.Equals(HttpStatusCode.Found))
        {
            string redirectUrl = responseMessage.Headers.Location.AbsoluteUri;

            responseMessage.Dispose();
            GC.Collect();

            return await HttpGetAsync(redirectUrl, authorization, httpCompletionOption);
        }

        return responseMessage;
    }

    public static async Task<HttpResponseMessage> HttpGetAsync(string url, Dictionary<string, string> headers, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

        if (headers != null && headers.Any())
            foreach (var kvp in headers)
                requestMessage.Headers.Add(kvp.Key, kvp.Value);

        var responseMessage = await HttpClient.SendAsync(requestMessage, httpCompletionOption, CancellationToken.None);

        if (responseMessage.StatusCode.Equals(HttpStatusCode.Found))
        {
            string redirectUrl = responseMessage.Headers.Location.AbsoluteUri;

            responseMessage.Dispose();
            GC.Collect();

            return await HttpGetAsync(redirectUrl, headers, httpCompletionOption);
        }

        return responseMessage;
    }

    public static async Task<HttpResponseMessage> HttpPostAsync(string url, Stream content, string contentType = "application/json")
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
        var httpContent = new StreamContent(content);

        httpContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        requestMessage.Content = httpContent;

        var res = await HttpClient.SendAsync(requestMessage);

        content.Dispose();
        httpContent.Dispose();

        return res;
    }

    public static async Task<HttpResponseMessage> HttpPostAsync(string url, string content, string contentType = "application/json")
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
        using var httpContent = new StringContent(content);

        httpContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        requestMessage.Content = httpContent;

        var res = await HttpClient.SendAsync(requestMessage);
        return res;
    }

    public static async Task<HttpResponseMessage> HttpPostAsync(string url, string content, Dictionary<string, string> headers, string contentType = "application/json")
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
        using var httpContent = new StringContent(content);
        httpContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        if (headers != null && headers.Any())
            foreach (var kvp in headers)
                requestMessage.Headers.Add(kvp.Key, kvp.Value);

        requestMessage.Content = httpContent;

        var res = await HttpClient.SendAsync(requestMessage);
        return res;
    }

    public static void SetTimeout(int milliseconds) => HttpClient.Timeout = new TimeSpan(0, 0, 0, 0, milliseconds);
}
