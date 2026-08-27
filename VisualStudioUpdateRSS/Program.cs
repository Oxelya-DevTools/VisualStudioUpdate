using System.Text.RegularExpressions;

/// <summary>
/// Runs the feed generation workflow for Visual Studio release notes.
/// </summary>
internal static class Program
{
    private const long MaxResponseBytes = 16L * 1024 * 1024;

    private static async Task<int> Main(string[] args)
    {
        var options = CommandLineOptions.Parse(args);
        if (options is null)
        {
            Console.Error.WriteLine(CommandLineOptions.UsageText);
            return 1;
        }

        using var httpClient = HttpClientFactory.Create(MaxResponseBytes);
        var definition = FeedSources.GetDefinition(options.Product);

        try
        {
            var entries = (await LoadReleasesAsync(httpClient, options.Product, definition))
                .DistinctBy(static release => (release.Published, release.Title, release.Link))
                .OrderByDescending(static release => release.Published)
                .ThenByDescending(static release => release.Title, StringComparer.Ordinal)
                .ToArray();

            if (entries.Length == 0)
            {
                throw new InvalidOperationException("No release entries were found in the Microsoft Learn pages.");
            }

            await AtomFeedWriter.WriteAsync(
                options.AtomOutputPath,
                entries,
                definition.AtomTitle,
                definition.AtomId,
                definition.AtomSelfLink);

            await TimelineMarkdownWriter.WriteAsync(
                options.TimelineOutputPath,
                entries,
                FeedSources.ReadmeUrl,
                definition.TimelineTitle,
                definition.TimelineChannels);

            Console.WriteLine(
                $"Generated {entries.Length} entries in {Path.GetFullPath(options.AtomOutputPath)} and {Path.GetFullPath(options.TimelineOutputPath)}");
            return 0;
        }
        catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException
            or IOException or TaskCanceledException or RegexMatchTimeoutException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"Unable to generate release artifacts: {exception.Message}");
            return 1;
        }
    }

    private static async Task<IReadOnlyList<ReleaseEntry>> LoadReleasesAsync(
        HttpClient httpClient,
        ProductKind product,
        FeedDefinition definition)
    {
        if (product == ProductKind.VisualStudioCode)
        {
            return await new VisualStudioCodeReleaseLoader(MaxResponseBytes).LoadReleasesAsync(httpClient);
        }

        var parser = new ReleaseNotesParser(MaxResponseBytes);
        var releases = await Task.WhenAll(
            definition.Sources.Select(source => parser.LoadReleasesAsync(httpClient, source.Url, source.ProductName)));
        return releases.SelectMany(static sourceReleases => sourceReleases).ToArray();
    }
}
