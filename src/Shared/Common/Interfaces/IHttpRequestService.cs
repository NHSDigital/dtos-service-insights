namespace Common;

using System.Net;

public interface IHttpRequestService
{
    Task<HttpResponseMessage> SendPost(string url, string postData);
}
