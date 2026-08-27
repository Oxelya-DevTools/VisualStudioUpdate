/// <summary>
/// Defines a channel displayed in a Markdown timeline.
/// </summary>
/// <param name="Name">Channel display name.</param>
/// <param name="StyleTag">Mermaid task style.</param>
internal sealed record TimelineChannel(string Name, string StyleTag);
