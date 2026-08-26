# VisualStudioUpdate

[![Publish Visual Studio Atom feed](https://github.com/Oxelya-DevTools/VisualStudioUpdate/actions/workflows/publish-feed.yml/badge.svg)](https://github.com/Oxelya-DevTools/VisualStudioUpdate/actions/workflows/publish-feed.yml)

This .NET 10 console application generates an Atom feed containing the release updates documented for Visual Studio 2026, Visual Studio 2026 Insiders, and Visual Studio 2026 Build Tools.

The application reads the official Microsoft Learn release-note pages instead of the Visual Studio blog RSS feed, because the blog feed does not document every release:

- https://learn.microsoft.com/en-us/visualstudio/releases/2026/release-notes
- https://learn.microsoft.com/en-us/visualstudio/releases/2026/release-notes-insiders

## Usage

Run the command from the repository root:

```powershell
dotnet run --project .\VisualStudioUpdateRSS\VisualStudioUpdateRSS.csproj -- --output .\visual-studio-2026.atom
```

If you are not in the repository root, use the full path to the project and output file. The default output file is `visual-studio-2026.atom`. The generated Atom document can be added directly to an RSS/Atom reader.

## Use the public feed or host your own

You have two options:

1. Use the already published feed directly:

```text
https://oxelya-devtools.github.io/VisualStudioUpdate/visual-studio-2026.atom
```

2. Publish the feed in your own repository:
   - Fork or duplicate this repository.
   - Push it to GitHub.
   - Open **Settings > Pages**.
   - Set **Source** to **GitHub Actions**.
   - Run the **Publish Visual Studio Atom feed** workflow once from the **Actions** tab.
   - Subscribe to your own GitHub Pages URL, for example:

```text
https://YOUR-OWNER.github.io/YOUR-REPOSITORY/visual-studio-2026.atom
```

The included `.github/workflows/publish-feed.yml` regenerates the feed every day and keeps it updated automatically.
