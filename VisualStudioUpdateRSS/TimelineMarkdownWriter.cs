using System.Globalization;
using System.Text;

/// <summary>
/// Writes Markdown timeline output files with Mermaid diagrams.
/// </summary>
internal static class TimelineMarkdownWriter
{
    /// <summary>
    /// Writes a Markdown timeline document.
    /// </summary>
    /// <param name="outputPath">Target file path.</param>
    /// <param name="entries">Release entries used for timeline and table sections.</param>
    /// <param name="readmeUrl">Repository README URL.</param>
    /// <param name="timelineTitle">Timeline document and Mermaid chart title.</param>
    /// <param name="channels">Channels included in the Mermaid chart.</param>
    public static async Task WriteAsync(
        string outputPath,
        IReadOnlyList<ReleaseEntry> entries,
        string readmeUrl,
        string timelineTitle,
        IReadOnlyList<TimelineChannel> channels)
    {
        var fullPath = Path.GetFullPath(outputPath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var usCulture = CultureInfo.GetCultureInfo("en-US");
        var latestPublished = entries[0].Published;
        var firstQuarterStart = GetQuarterStart(latestPublished);
        var secondQuarterStart = firstQuarterStart.AddMonths(3);
        var timelineEndExclusive = firstQuarterStart.AddMonths(6);

        var builder = new StringBuilder();
        builder.AppendLine($"# {timelineTitle}");
        builder.AppendLine();
        builder.AppendLine($"Updated: {latestPublished:yyyy-MM-ddTHH:mm:ss.fffffffK} (UTC)");
        builder.AppendLine();
        builder.AppendLine("Roadmap window: **only months with published releases**.");
        builder.AppendLine();
        builder.AppendLine($"Repository README: [VisualStudioUpdate README]({readmeUrl})");
        builder.AppendLine();
        builder.AppendLine("```mermaid");
        builder.AppendLine("%%{init: {'theme':'dark','themeVariables': { 'fontFamily': 'Segoe UI', 'fontSize': '24px', 'textColor': '#F3F4F6', 'titleTextColor': '#FFFFFF', 'taskTextColor': '#111827', 'taskTextLightColor': '#111827', 'taskTextOutsideColor': '#F3F4F6', 'taskBkgColor': '#BAE6FD', 'taskBorderColor': '#0369A1', 'doneTaskBkgColor': '#E9D5FF', 'doneTaskBorderColor': '#7E22CE', 'activeTaskBkgColor': '#99F6E4', 'activeTaskBorderColor': '#0F766E', 'critTaskBkgColor': '#FED7AA', 'critTaskBorderColor': '#C2410C', 'sectionBkgColor': '#1F2937', 'altSectionBkgColor': '#111827', 'gridColor': '#4B5563', 'todayLineColor': '#F87171' }}}%%");
        builder.AppendLine("gantt");
        builder.AppendLine($"    title {timelineTitle}");
        builder.AppendLine("    dateFormat  YYYY-MM-DD");
        builder.AppendLine("    axisFormat  %b");
        builder.AppendLine("    tickInterval 1month");
        foreach (var channel in channels)
        {
            builder.AppendLine($"    section {channel.Name}");
            AppendChannelGanttSection(
                builder,
                entries,
                channel.Name,
                channel.StyleTag,
                firstQuarterStart,
                timelineEndExclusive,
                latestPublished,
                usCulture);
        }
        builder.AppendLine("```");
        builder.AppendLine();
        builder.AppendLine("## Releases");
        builder.AppendLine();
        builder.AppendLine("| Date (UTC) | Channel | Version | Release notes |");
        builder.AppendLine("| --- | --- | --- | --- |");

        foreach (var entry in entries)
        {
            var (channel, version) = ReleaseChannelParser.GetChannelAndVersion(entry.Title);
            builder.AppendLine(
                $"| {entry.Published.UtcDateTime.ToString("yyyy-MM-dd", usCulture)} | {channel} | {EscapeTable(version)} | [Open]({entry.Link}) |");
        }

        await File.WriteAllTextAsync(fullPath, builder.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static void AppendChannelGanttSection(
        StringBuilder builder,
        IReadOnlyList<ReleaseEntry> entries,
        string channel,
        string styleTag,
        DateTimeOffset firstQuarterStart,
        DateTimeOffset timelineEndExclusive,
        DateTimeOffset latestPublished,
        CultureInfo usCulture)
    {
        var versionItems = entries
            .Select(static entry =>
            {
                var (entryChannel, version) = ReleaseChannelParser.GetChannelAndVersion(entry.Title);
                return new { Entry = entry, Channel = entryChannel, Version = NormalizeVersionLabel(version) };
            })
            .Where(item => string.Equals(item.Channel, channel, StringComparison.Ordinal))
            .Where(item => item.Entry.Published >= firstQuarterStart && item.Entry.Published < timelineEndExclusive)
            .GroupBy(item => item.Version, StringComparer.Ordinal)
            .Select(static group => group
                .OrderBy(item => item.Entry.Published)
                .ThenBy(item => item.Entry.Title, StringComparer.Ordinal)
                .First())
            .OrderBy(item => item.Entry.Published)
            .ThenBy(item => item.Version, StringComparer.Ordinal)
            .ToArray();

        if (versionItems.Length == 0)
        {
            builder.AppendLine($"    Planned updates : milestone, {channel.Replace(" ", string.Empty, StringComparison.Ordinal).ToLowerInvariant()}planned, {firstQuarterStart:yyyy-MM-dd}, 1d");
            return;
        }

        for (var index = 0; index < versionItems.Length; index++)
        {
            var versionItem = versionItems[index];
            var releaseDate = versionItem.Entry.Published;
            var hasCurrent = releaseDate.UtcDateTime.Date == latestPublished.UtcDateTime.Date;
            var currentMarker = hasCurrent ? " (current)" : string.Empty;
            var label = $"{releaseDate.UtcDateTime.ToString("MMM dd", usCulture)} · {versionItem.Version}{currentMarker}";
            var taskId = $"{channel.Replace(" ", string.Empty, StringComparison.Ordinal).ToLowerInvariant()}{releaseDate:yyyyMMdd}{index}";
            builder.AppendLine($"    {label} : {styleTag}, {taskId}, {releaseDate:yyyy-MM-dd}, 20d");
        }
    }

    private static DateTimeOffset GetQuarterStart(DateTimeOffset date)
    {
        var quarterStartMonth = ((date.Month - 1) / 3) * 3 + 1;
        return new DateTimeOffset(date.Year, quarterStartMonth, 1, 0, 0, 0, TimeSpan.Zero);
    }

    private static string EscapeTable(string value)
    {
        return value.Replace("|", "\\|", StringComparison.Ordinal);
    }

    private static string NormalizeVersionLabel(string version)
    {
        return string.IsNullOrWhiteSpace(version) ? "Unlabeled release" : version.Trim();
    }
}
