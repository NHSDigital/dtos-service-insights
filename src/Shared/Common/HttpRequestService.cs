namespace Common;

using System.Text;

public class HttpRequestService : IHttpRequestService
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public async Task<HttpResponseMessage> SendPost(string url, string postData)
    {
        return await SendHttpRequestAsync(url, postData, HttpMethod.Post);
    }

    private async Task<HttpResponseMessage> SendHttpRequestAsync(string url, string dataToSend, HttpMethod method)
    {
        using var request = new HttpRequestMessage(method, url)
        {
            Content = new StringContent(dataToSend, Encoding.UTF8, "application/json")
        };

        HttpResponseMessage response;

        response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return response;
    }
}
