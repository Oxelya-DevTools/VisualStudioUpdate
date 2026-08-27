using System.Text.Json;

/// <summary>
/// Loads Visual Studio Code stable and Insiders releases from the official Microsoft GitHub repository.
/// </summary>
internal sealed class VisualStudioCodeReleaseLoader
{
    private const string ReleasesUrl = "https://api.github.com/repos/microsoft/vscode/releases?per_page=30";
    private const string InsidersUrl = "https://api.github.com/repos/microsoft/vscode/commits?sha=main&per_page=30";
    private readonly long maxResponseBytes;

    /// <summary>
    /// Initializes a new Visual Studio Code release loader.
    /// </summary>
    /// <param name="maxResponseBytes">Maximum allowed response body size in bytes.</param>
    public VisualStudioCodeReleaseLoader(long maxResponseBytes)
    {
        this.maxResponseBytes = maxResponseBytes;
    }

    /// <summary>
    /// Loads stable releases and daily Insiders builds.
    /// </summary>
    /// <param name="client">HTTP client used for the downloads.</param>
    /// <returns>Normalized Visual Studio Code releases.</returns>
    public async Task<IReadOnlyList<ReleaseEntry>> LoadReleasesAsync(HttpClient client)
    {
        var stableTask = LoadStableReleasesAsync(client);
        var insidersTask = LoadInsidersReleasesAsync(client);
        await Task.WhenAll(stableTask, insidersTask);

        return stableTask.Result.Concat(insidersTask.Result).ToArray();
    }

    private async Task<IReadOnlyList<ReleaseEntry>> LoadStableReleasesAsync(HttpClient client)
    {
        using var document = await LoadDocumentAsync(client, ReleasesUrl);
        var releases = new List<ReleaseEntry>();

        foreach (var release in document.RootElement.EnumerateArray())
        {
            if (release.GetProperty("prerelease").GetBoolean() || release.GetProperty("draft").GetBoolean())
            {
                continue;
            }

            var version = release.GetProperty("tag_name").GetString();
            var published = release.GetProperty("published_at").GetDateTimeOffset();
            var link = release.GetProperty("html_url").GetString();
            if (string.IsNullOrWhiteSpace(version) || string.IsNullOrWhiteSpace(link))
            {
                throw new InvalidOperationException("A Visual Studio Code stable release is missing its version or URL.");
            }

            if (!Version.TryParse(version, out var parsedVersion) || parsedVersion.Major < 1)
            {
                continue;
            }

            releases.Add(new($"VS Code: Version {version}", published, link, $"Visual Studio Code stable release {version}."));
        }

        return releases;
    }

    private async Task<IReadOnlyList<ReleaseEntry>> LoadInsidersReleasesAsync(HttpClient client)
    {
        using var document = await LoadDocumentAsync(client, InsidersUrl);
        var releases = new List<ReleaseEntry>();

        var commits = document.RootElement
            .EnumerateArray()
            .Select(commit => new
            {
                Sha = commit.GetProperty("sha").GetString(),
                Link = commit.GetProperty("html_url").GetString(),
                Published = commit.GetProperty("commit").GetProperty("author").GetProperty("date").GetDateTimeOffset(),
                Message = commit.GetProperty("commit").GetProperty("message").GetString()?.Split('\n')[0].Trim(),
            })
            .GroupBy(commit => commit.Published.UtcDateTime.Date)
            .Select(static group => group.OrderByDescending(commit => commit.Published).First());

        foreach (var commit in commits)
        {
            if (string.IsNullOrWhiteSpace(commit.Sha) || string.IsNullOrWhiteSpace(commit.Link))
            {
                throw new InvalidOperationException("A Visual Studio Code Insiders build is missing its commit or URL.");
            }

            var build = commit.Sha[..Math.Min(8, commit.Sha.Length)];
            releases.Add(new(
                $"VS Code Insiders: Build {build}",
                commit.Published,
                commit.Link,
                string.IsNullOrWhiteSpace(commit.Message) ? $"Visual Studio Code Insiders build {build}." : commit.Message));
        }

        return releases;
    }

    private async Task<JsonDocument> LoadDocumentAsync(HttpClient client, string url)
    {
        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var contentLength = response.Content.Headers.ContentLength;
        if (contentLength > this.maxResponseBytes)
        {
            throw new InvalidOperationException($"The response from {url} exceeds the {this.maxResponseBytes} byte limit.");
        }

        await using var stream = await response.Content.ReadAsStreamAsync();
        return await JsonDocument.ParseAsync(stream);
    }
}
