/// <summary>
/// Defines source endpoints and metadata for feed generation.
/// </summary>
internal static class FeedSources
{
    /// <summary>
    /// Gets the Atom feed title.
    /// </summary>
    public const string AtomTitle = "Visual Studio 2026 Updates";

    /// <summary>
    /// Gets the Atom feed identifier.
    /// </summary>
    public const string AtomId = "https://learn.microsoft.com/en-us/visualstudio/releases/2026/release-notes";

    /// <summary>
    /// Gets the Atom self-link value.
    /// </summary>
    public const string AtomSelfLink = "https://learn.microsoft.com/en-us/visualstudio/releases/2026/release-notes";

    /// <summary>
    /// Gets the public README URL.
    /// </summary>
    public const string ReadmeUrl = "https://github.com/Oxelya-DevTools/VisualStudioUpdate/blob/master/README.md";

    /// <summary>
    /// Gets all release-note sources included in the generated output.
    /// </summary>
    public static IReadOnlyList<FeedSource> All { get; } =
    [
        new("https://learn.microsoft.com/en-us/visualstudio/releases/2026/release-notes", "Visual Studio 2026"),
        new("https://learn.microsoft.com/en-us/visualstudio/releases/2026/release-notes-insiders", "Visual Studio 2026 Insiders"),
        new("https://learn.microsoft.com/en-us/visualstudio/releases/2026/release-notes?tabs=buildtools", "Visual Studio 2026 Build Tools"),
    ];

    /// <summary>
    /// Gets metadata for Visual Studio 2026 output.
    /// </summary>
    public static FeedDefinition VisualStudio { get; } = new(
        AtomTitle,
        AtomId,
        AtomSelfLink,
        "Visual Studio 2026 Timeline",
        All,
        [new("VS 2026", "done"), new("Insiders", "active"), new("Build Tools", "crit")]);

    /// <summary>
    /// Gets metadata for Visual Studio Code output.
    /// </summary>
    public static FeedDefinition VisualStudioCode { get; } = new(
        "Visual Studio Code Updates",
        "https://oxelya-devtools.github.io/VisualStudioUpdate/vscode.atom",
        "https://oxelya-devtools.github.io/VisualStudioUpdate/vscode.atom",
        "Visual Studio Code Timeline",
        [],
        [new("VS Code", "done"), new("VS Code Insiders", "active")]);

    /// <summary>
    /// Gets the metadata for the specified product.
    /// </summary>
    /// <param name="product">Product to resolve.</param>
    /// <returns>The associated feed definition.</returns>
    public static FeedDefinition GetDefinition(ProductKind product)
    {
        return product == ProductKind.VisualStudio ? VisualStudio : VisualStudioCode;
    }
}

/// <summary>
/// Represents one release-note source to parse.
/// </summary>
/// <param name="Url">Source URL.</param>
/// <param name="ProductName">Product prefix used in generated titles.</param>
internal sealed record FeedSource(string Url, string ProductName);

/// <summary>
/// Defines output metadata and sources for one product feed.
/// </summary>
/// <param name="AtomTitle">Atom feed title.</param>
/// <param name="AtomId">Atom feed identifier.</param>
/// <param name="AtomSelfLink">Public Atom self-link.</param>
/// <param name="TimelineTitle">Markdown timeline title.</param>
/// <param name="Sources">HTML sources used for release parsing.</param>
/// <param name="TimelineChannels">Channels included in the Mermaid timeline.</param>
internal sealed record FeedDefinition(
    string AtomTitle,
    string AtomId,
    string AtomSelfLink,
    string TimelineTitle,
    IReadOnlyList<FeedSource> Sources,
    IReadOnlyList<TimelineChannel> TimelineChannels);
