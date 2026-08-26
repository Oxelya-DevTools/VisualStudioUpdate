using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

internal static partial class Program
{
    private const string VisualStudioReleaseNotes = "https://learn.microsoft.com/en-us/visualstudio/releases/2026/release-notes";
    private const string VisualStudioInsiderReleaseNotes = "https://learn.microsoft.com/en-us/visualstudio/releases/2026/release-notes-insiders";
    private const string DefaultOutputPath = "visual-studio-2026.atom";
    private const int RegexTimeoutMilliseconds = 5_000;
    private const long MaxResponseBytes = 16L * 1024 * 1024;

    private static async Task<int> Main(string[] args)
    {
        var outputPath = GetOutputPath(args);
        if (outputPath is null)
        {
            Console.Error.WriteLine("Usage: VisualStudioUpdateRSS [--output <path>]");
            return 1;
        }

        using var httpClient = CreateHttpClient();

        try
        {
            var entries = (await Task.WhenAll(
                    LoadReleasesAsync(httpClient, VisualStudioReleaseNotes, "Visual Studio 2026"),
                    LoadReleasesAsync(httpClient, VisualStudioInsiderReleaseNotes, "Visual Studio 2026 Insiders")))
                .SelectMany(static releases => releases)
                .OrderByDescending(static release => release.Published)
                .ThenByDescending(static release => release.Title, StringComparer.Ordinal)
                .ToArray();

            if (entries.Length == 0)
            {
                throw new InvalidOperationException("No release entries were found in the Microsoft Learn pages.");
            }

            await WriteAtomFeedAsync(outputPath, entries);
            Console.WriteLine($"Generated {entries.Length} entries in {Path.GetFullPath(outputPath)}");
            return 0;
        }
        catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException
            or IOException or TaskCanceledException or RegexMatchTimeoutException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"Unable to generate the Atom feed: {exception.Message}");
            return 1;
        }
    }

    private static HttpClient CreateHttpClient()
    {
        var handler = new SocketsHttpHandler { AllowAutoRedirect = true, MaxAutomaticRedirections = 5 };
        var client = new HttpClient(handler, disposeHandler: true)
        {
            Timeout = TimeSpan.FromSeconds(30),
            MaxResponseContentBufferSize = MaxResponseBytes,
        };
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("VisualStudioUpdateRSS", "1.0"));
        return client;
    }

    private static async Task<IReadOnlyList<ReleaseEntry>> LoadReleasesAsync(HttpClient client, string url, string product)
    {
        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var contentLength = response.Content.Headers.ContentLength;
        if (contentLength > MaxResponseBytes)
        {
            throw new InvalidOperationException($"The response from {url} exceeds the {MaxResponseBytes} byte limit.");
        }

        var html = await response.Content.ReadAsStringAsync();
        var headings = HeadingRegex().Matches(html).Cast<Match>().ToArray();
        var releases = new List<ReleaseEntry>();

        for (var index = 0; index < headings.Length; index++)
        {
            var heading = headings[index];
            var title = CleanText(heading.Groups[2].Value);
            if (!title.StartsWith("Version ", StringComparison.OrdinalIgnoreCase) &&
                !title.Contains("Update ", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var contentStart = heading.Index + heading.Length;
            var contentEnd = index + 1 < headings.Length ? headings[index + 1].Index : html.Length;
            var section = html[contentStart..contentEnd];
            var dateMatch = DateRegex().Match(section);
            if (!dateMatch.Success || !DateTimeOffset.TryParse(
                    CleanText(dateMatch.Groups[1].Value), CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal, out var published))
            {
                throw new InvalidOperationException($"The release date is missing or invalid in {url} for '{title}'.");
            }

            var summary = CleanText(section);
            releases.Add(new ReleaseEntry(
                $"{product}: {title}",
                published,
                $"{url}#{heading.Groups[1].Value}",
                summary));
        }

        return releases;
    }

    private static async Task WriteAtomFeedAsync(string outputPath, IReadOnlyList<ReleaseEntry> entries)
    {
        var fullPath = Path.GetFullPath(outputPath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var settings = new XmlWriterSettings { Async = true, Encoding = new UTF8Encoding(false), Indent = true };
        await using var stream = File.Create(fullPath);
        await using var writer = XmlWriter.Create(stream, settings);
        await writer.WriteStartDocumentAsync();
        await writer.WriteStartElementAsync(null, "feed", "http://www.w3.org/2005/Atom");
        await writer.WriteElementStringAsync(null, "title", null, "Visual Studio 2026 Updates");
        await writer.WriteElementStringAsync(null, "id", null, "https://learn.microsoft.com/en-us/visualstudio/releases/2026/release-notes");
        await writer.WriteElementStringAsync(null, "updated", null, entries[0].Published.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
        await writer.WriteStartElementAsync(null, "link", null);
        await writer.WriteAttributeStringAsync(null, "rel", null, "self");
        await writer.WriteAttributeStringAsync(null, "href", null, "https://learn.microsoft.com/en-us/visualstudio/releases/2026/release-notes");
        await writer.WriteEndElementAsync();

        foreach (var entry in entries)
        {
            await writer.WriteStartElementAsync(null, "entry", null);
            await writer.WriteElementStringAsync(null, "title", null, entry.Title);
            await writer.WriteElementStringAsync(null, "id", null, entry.Link);
            await writer.WriteStartElementAsync(null, "link", null);
            await writer.WriteAttributeStringAsync(null, "href", null, entry.Link);
            await writer.WriteEndElementAsync();
            await writer.WriteElementStringAsync(null, "published", null, entry.Published.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
            await writer.WriteElementStringAsync(null, "updated", null, entry.Published.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
            await writer.WriteElementStringAsync(null, "summary", null, entry.Summary);
            await writer.WriteEndElementAsync();
        }

        await writer.WriteEndElementAsync();
        await writer.WriteEndDocumentAsync();
    }

    private static string? GetOutputPath(string[] args)
    {
        for (var index = 0; index < args.Length; index++)
        {
            if (!string.Equals(args[index], "--output", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (index + 1 >= args.Length || string.IsNullOrWhiteSpace(args[index + 1]))
            {
                return null;
            }

            return args[index + 1];
        }

        return DefaultOutputPath;
    }

    private static string CleanText(string html)
    {
        var withoutCode = ScriptAndStyleRegex().Replace(html, " ");
        var text = TagRegex().Replace(withoutCode, " ");
        var decoded = InvalidXmlCharRegex().Replace(WebUtility.HtmlDecode(text), string.Empty);
        return WhitespaceRegex().Replace(decoded, " ").Trim();
    }

    [GeneratedRegex("<h2\\b[^>]*id=\\\"([^\\\"]+)\\\"[^>]*>(.*?)</h2>", RegexOptions.IgnoreCase | RegexOptions.Singleline, RegexTimeoutMilliseconds)]
    private static partial Regex HeadingRegex();

    [GeneratedRegex("Released on\\s*(?:<[^>]+>\\s*)*([^<]+)", RegexOptions.IgnoreCase | RegexOptions.Singleline, RegexTimeoutMilliseconds)]
    private static partial Regex DateRegex();

    [GeneratedRegex("<(script|style)\\b[^>]*>.*?</\\1>", RegexOptions.IgnoreCase | RegexOptions.Singleline, RegexTimeoutMilliseconds)]
    private static partial Regex ScriptAndStyleRegex();

    [GeneratedRegex("<[^>]+>", RegexOptions.None, RegexTimeoutMilliseconds)]
    private static partial Regex TagRegex();

    [GeneratedRegex("\\s+", RegexOptions.None, RegexTimeoutMilliseconds)]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex("[\\x00-\\x08\\x0B\\x0C\\x0E-\\x1F\\uFFFE\\uFFFF]", RegexOptions.None, RegexTimeoutMilliseconds)]
    private static partial Regex InvalidXmlCharRegex();

    private sealed record ReleaseEntry(string Title, DateTimeOffset Published, string Link, string Summary);
}
