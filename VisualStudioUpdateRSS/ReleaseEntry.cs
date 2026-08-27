/// <summary>
/// Represents one normalized release entry.
/// </summary>
/// <param name="Title">Display title for the release.</param>
/// <param name="Published">Release publication timestamp.</param>
/// <param name="Link">Release notes URL.</param>
/// <param name="Summary">Release summary text.</param>
internal sealed record ReleaseEntry(string Title, DateTimeOffset Published, string Link, string Summary);
