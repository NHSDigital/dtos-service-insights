using System.Text;

namespace NHS.ServiceInsights.Common;

public class HttpRequestService : IHttpRequestService
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public async Task<HttpResponseMessage> SendPost(string url, string postData)
    {
        return await SendHttpRequestAsync(url, postData, HttpMethod.Post);
    }

    public async Task<HttpResponseMessage> SendGet(string url)
    {
        var response = await _httpClient.GetAsync(url);

        return response;

    }

    public async Task<HttpResponseMessage> SendPut(string url, string putData)
    {
        return await SendHttpRequestAsync(url, putData, HttpMethod.Put);
    }

    private async Task<HttpResponseMessage> SendHttpRequestAsync(string url, string dataToSend, HttpMethod method)
    {
        using var request = new HttpRequestMessage(method, url)
        {
            Content = new StringContent(dataToSend, Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return response;
    }
}
