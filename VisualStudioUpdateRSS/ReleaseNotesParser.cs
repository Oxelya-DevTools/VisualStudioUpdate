using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

/// <summary>
/// Parses Microsoft Learn release-note pages into normalized release entries.
/// </summary>
internal sealed partial class ReleaseNotesParser
{
    private const int RegexTimeoutMilliseconds = 5_000;
    private readonly long maxResponseBytes;

    /// <summary>
    /// Initializes a new parser instance.
    /// </summary>
    /// <param name="maxResponseBytes">Maximum allowed response body size in bytes.</param>
    public ReleaseNotesParser(long maxResponseBytes)
    {
        this.maxResponseBytes = maxResponseBytes;
    }

    /// <summary>
    /// Loads and parses release entries from a single source page.
    /// </summary>
    /// <param name="client">HTTP client used for the download.</param>
    /// <param name="url">Source page URL.</param>
    /// <param name="product">Product label prefix for generated titles.</param>
    /// <returns>A list of parsed release entries.</returns>
    public async Task<IReadOnlyList<ReleaseEntry>> LoadReleasesAsync(HttpClient client, string url, string product)
    {
        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var contentLength = response.Content.Headers.ContentLength;
        if (contentLength > this.maxResponseBytes)
        {
            throw new InvalidOperationException($"The response from {url} exceeds the {this.maxResponseBytes} byte limit.");
        }

        var html = await response.Content.ReadAsStringAsync();
        var headings = HeadingRegex().Matches(html).Cast<Match>().ToArray();
        var releases = new List<ReleaseEntry>();

        for (var index = 0; index < headings.Length; index++)
        {
            var heading = headings[index];
            var title = CleanText(heading.Groups[2].Value);
            if (!title.StartsWith("Version ", StringComparison.OrdinalIgnoreCase)
                && !title.Contains("Update ", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var contentStart = heading.Index + heading.Length;
            var contentEnd = index + 1 < headings.Length ? headings[index + 1].Index : html.Length;
            var section = html[contentStart..contentEnd];
            var dateMatch = DateRegex().Match(section);
            if (!dateMatch.Success || !DateTimeOffset.TryParse(
                    CleanText(dateMatch.Groups[1].Value),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal,
                    out var published))
            {
                throw new InvalidOperationException($"The release date is missing or invalid in {url} for '{title}'.");
            }

            releases.Add(
                new ReleaseEntry(
                    $"{product}: {title}",
                    published,
                    $"{url}#{heading.Groups[1].Value}",
                    CleanText(section)));
        }

        return releases;
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
}
