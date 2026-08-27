using System.Globalization;
using System.Text;
using System.Xml;

/// <summary>
/// Writes Atom feed output files.
/// </summary>
internal static class AtomFeedWriter
{
    /// <summary>
    /// Writes an Atom feed document to the specified output path.
    /// </summary>
    /// <param name="outputPath">Target file path.</param>
    /// <param name="entries">Feed entries to serialize.</param>
    /// <param name="feedTitle">Feed title.</param>
    /// <param name="feedId">Feed identifier.</param>
    /// <param name="feedSelfLink">Feed self-link URL.</param>
    public static async Task WriteAsync(
        string outputPath,
        IReadOnlyList<ReleaseEntry> entries,
        string feedTitle,
        string feedId,
        string feedSelfLink)
    {
        var fullPath = Path.GetFullPath(outputPath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var settings = new XmlWriterSettings
        {
            Async = true,
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            Indent = true,
        };

        await using var stream = File.Create(fullPath);
        await using var writer = XmlWriter.Create(stream, settings);

        await writer.WriteStartDocumentAsync();
        await writer.WriteStartElementAsync(null, "feed", "http://www.w3.org/2005/Atom");
        await writer.WriteElementStringAsync(null, "title", null, feedTitle);
        await writer.WriteElementStringAsync(null, "id", null, feedId);
        await writer.WriteElementStringAsync(
            null,
            "updated",
            null,
            entries[0].Published.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));

        await writer.WriteStartElementAsync(null, "link", null);
        await writer.WriteAttributeStringAsync(null, "rel", null, "self");
        await writer.WriteAttributeStringAsync(null, "href", null, feedSelfLink);
        await writer.WriteEndElementAsync();

        foreach (var entry in entries)
        {
            await writer.WriteStartElementAsync(null, "entry", null);
            await writer.WriteElementStringAsync(null, "title", null, entry.Title);
            await writer.WriteElementStringAsync(null, "id", null, entry.Link);
            await writer.WriteStartElementAsync(null, "link", null);
            await writer.WriteAttributeStringAsync(null, "href", null, entry.Link);
            await writer.WriteEndElementAsync();
            await writer.WriteElementStringAsync(
                null,
                "published",
                null,
                entry.Published.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
            await writer.WriteElementStringAsync(
                null,
                "updated",
                null,
                entry.Published.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
            await writer.WriteElementStringAsync(null, "summary", null, entry.Summary);
            await writer.WriteEndElementAsync();
        }

        await writer.WriteEndElementAsync();
        await writer.WriteEndDocumentAsync();
    }
}
