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
}

/// <summary>
/// Represents one release-note source to parse.
/// </summary>
/// <param name="Url">Source URL.</param>
/// <param name="ProductName">Product prefix used in generated titles.</param>
internal sealed record FeedSource(string Url, string ProductName);
