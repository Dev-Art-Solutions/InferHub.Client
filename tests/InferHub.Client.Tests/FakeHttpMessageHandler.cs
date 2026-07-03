using System.Net;

namespace InferHub.Client.Tests;

/// <summary>
/// Test double for <see cref="HttpMessageHandler"/>. Records every request and returns a
/// programmable response body/status. Keeps the tests dependency-free (no Moq).
/// </summary>
internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode statusCode;
    private readonly string responseBody;
    private readonly string mediaType;

    public FakeHttpMessageHandler(HttpStatusCode statusCode, string responseBody, string mediaType = "application/json")
    {
        this.statusCode = statusCode;
        this.responseBody = responseBody;
        this.mediaType = mediaType;
    }

    public List<HttpRequestMessage> Requests { get; } = new();

    public List<string> RequestBodies { get; } = new();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        if (request.Content is not null)
        {
            RequestBodies.Add(await request.Content.ReadAsStringAsync(cancellationToken));
        }
        else
        {
            RequestBodies.Add(string.Empty);
        }

        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(responseBody, System.Text.Encoding.UTF8, mediaType)
        };
    }
}
