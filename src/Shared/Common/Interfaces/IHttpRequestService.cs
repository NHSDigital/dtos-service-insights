namespace NHS.ServiceInsights.Common;

public interface IHttpRequestService
{
    Task<HttpResponseMessage> SendPost(string url, string postData);
}
