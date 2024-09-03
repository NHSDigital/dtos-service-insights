namespace NHS.ServiceInsights.Common;

public interface IHttpRequestService
{
    Task<HttpResponseMessage> SendGet(string url);
    Task<HttpResponseMessage> SendPost(string url, string postData);
}
