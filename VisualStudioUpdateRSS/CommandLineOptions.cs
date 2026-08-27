/// <summary>
/// Represents command-line options used by the generator.
/// </summary>
/// <param name="AtomOutputPath">Output path for the Atom file.</param>
/// <param name="TimelineOutputPath">Output path for the Markdown timeline file.</param>
/// <param name="Product">Product whose releases are generated.</param>
internal sealed record CommandLineOptions(string AtomOutputPath, string TimelineOutputPath, ProductKind Product)
{
    /// <summary>
    /// Gets the default Atom output path.
    /// </summary>
    public const string DefaultAtomOutputPath = "visual-studio-2026.atom";

    /// <summary>
    /// Gets the command-line usage message.
    /// </summary>
    public const string UsageText = "Usage: VisualStudioUpdateRSS [--product <visualstudio|vscode>] [--output <path>] [--timeline-output <path>]";

    /// <summary>
    /// Parses command-line arguments.
    /// </summary>
    /// <param name="args">Raw command-line arguments.</param>
    /// <returns>
    /// Parsed options when arguments are valid; otherwise <see langword="null" />.
    /// </returns>
    public static CommandLineOptions? Parse(string[] args)
    {
        var productValue = GetOptionValue(args, "--product");
        var product = productValue?.ToLowerInvariant() switch
        {
            null or "visualstudio" => ProductKind.VisualStudio,
            "vscode" => ProductKind.VisualStudioCode,
            _ => (ProductKind?)null,
        };

        if (product is null)
        {
            return null;
        }

        var atomOutputPath = GetOptionValue(args, "--output") ??
            (product == ProductKind.VisualStudio ? DefaultAtomOutputPath : "vscode.atom");
        var timelineOutputPath = GetOptionValue(args, "--timeline-output") ?? GetDefaultTimelineOutputPath(atomOutputPath);

        if (string.IsNullOrWhiteSpace(atomOutputPath) || string.IsNullOrWhiteSpace(timelineOutputPath))
        {
            return null;
        }

        return new CommandLineOptions(atomOutputPath, timelineOutputPath, product.Value);
    }

    private static string GetDefaultTimelineOutputPath(string atomOutputPath)
    {
        var fullAtomPath = Path.GetFullPath(atomOutputPath);
        var directory = Path.GetDirectoryName(fullAtomPath) ?? string.Empty;
        var timelineFileName = $"{Path.GetFileNameWithoutExtension(fullAtomPath)}.md";
        return Path.Combine(directory, timelineFileName);
    }

    private static string? GetOptionValue(string[] args, string optionName)
    {
        for (var index = 0; index < args.Length; index++)
        {
            if (!string.Equals(args[index], optionName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (index + 1 >= args.Length || string.IsNullOrWhiteSpace(args[index + 1]))
            {
                return null;
            }

            return args[index + 1];
        }

        return null;
    }
}
