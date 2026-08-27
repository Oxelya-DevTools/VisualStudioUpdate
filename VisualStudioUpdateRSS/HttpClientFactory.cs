using System.Net.Http.Headers;

/// <summary>
/// Creates configured <see cref="HttpClient" /> instances for release-note downloads.
/// </summary>
internal static class HttpClientFactory
{
    /// <summary>
    /// Creates an HTTP client configured for release-note scraping.
    /// </summary>
    /// <param name="maxResponseBytes">Maximum response payload size in bytes.</param>
    /// <returns>A configured HTTP client.</returns>
    public static HttpClient Create(long maxResponseBytes)
    {
        var handler = new SocketsHttpHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 5,
        };

        var client = new HttpClient(handler, disposeHandler: true)
        {
            Timeout = TimeSpan.FromSeconds(30),
            MaxResponseContentBufferSize = maxResponseBytes,
        };

        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("VisualStudioUpdateRSS", "1.0"));
        return client;
    }
}
