/// <summary>
/// Parses channel and version data from normalized entry titles.
/// </summary>
internal static class ReleaseChannelParser
{
    /// <summary>
    /// Parses a generated entry title into channel and version values.
    /// </summary>
    /// <param name="title">Generated entry title.</param>
    /// <returns>Tuple containing channel and version strings.</returns>
    public static (string Channel, string Version) GetChannelAndVersion(string title)
    {
        const string visualStudioPrefix = "Visual Studio 2026: ";
        const string insidersPrefix = "Visual Studio 2026 Insiders: ";
        const string buildToolsPrefix = "Visual Studio 2026 Build Tools: ";
        const string visualStudioCodePrefix = "VS Code: ";
        const string visualStudioCodeInsidersPrefix = "VS Code Insiders: ";

        if (title.StartsWith(visualStudioCodeInsidersPrefix, StringComparison.Ordinal))
        {
            return ("VS Code Insiders", title[visualStudioCodeInsidersPrefix.Length..]);
        }

        if (title.StartsWith(visualStudioCodePrefix, StringComparison.Ordinal))
        {
            return ("VS Code", title[visualStudioCodePrefix.Length..]);
        }

        if (title.StartsWith(buildToolsPrefix, StringComparison.Ordinal))
        {
            return ("Build Tools", title[buildToolsPrefix.Length..]);
        }

        if (title.StartsWith(insidersPrefix, StringComparison.Ordinal))
        {
            return ("Insiders", title[insidersPrefix.Length..]);
        }

        if (title.StartsWith(visualStudioPrefix, StringComparison.Ordinal))
        {
            return ("VS 2026", title[visualStudioPrefix.Length..]);
        }

        return ("Visual Studio", title);
    }
}
