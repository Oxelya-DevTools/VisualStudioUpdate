# VisualStudioUpdate

[![Publish update feeds](https://github.com/Oxelya-DevTools/VisualStudioUpdate/actions/workflows/publish-feed.yml/badge.svg)](https://github.com/Oxelya-DevTools/VisualStudioUpdate/actions/workflows/publish-feed.yml)

This .NET 10 console application generates Atom feeds and Markdown timelines for Visual Studio 2026 and Visual Studio Code. The VS Code feed includes GA releases and daily Insiders builds.

The application reads the official Microsoft Learn release-note pages instead of the Visual Studio blog RSS feed, because the blog feed does not document every release:

- https://learn.microsoft.com/en-us/visualstudio/releases/2026/release-notes
- https://learn.microsoft.com/en-us/visualstudio/releases/2026/release-notes-insiders

## Usage

Run the command from the repository root:

```powershell
dotnet run --project .\VisualStudioUpdateRSS\VisualStudioUpdateRSS.csproj -- --output .\visual-studio-2026.atom
dotnet run --project .\VisualStudioUpdateRSS\VisualStudioUpdateRSS.csproj -- --product vscode --output .\vscode.atom
```

If you are not in the repository root, use the full path to the project and output file. The default output files are `visual-studio-2026.atom` and `visual-studio-2026.md` in the same folder. You can override the Markdown path with `--timeline-output <path>`.

## Use the public feed or host your own

You have two options:

1. Use the already published files directly:

```text
https://oxelya-devtools.github.io/VisualStudioUpdate/visual-studio-2026.atom
https://oxelya-devtools.github.io/VisualStudioUpdate/visual-studio-2026.md
https://oxelya-devtools.github.io/VisualStudioUpdate/vscode.atom
https://oxelya-devtools.github.io/VisualStudioUpdate/vscode.md
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
https://YOUR-OWNER.github.io/YOUR-REPOSITORY/visual-studio-2026.md
https://YOUR-OWNER.github.io/YOUR-REPOSITORY/vscode.atom
https://YOUR-OWNER.github.io/YOUR-REPOSITORY/vscode.md
```

The included `.github/workflows/publish-feed.yml` regenerates all four files every day and verifies that each output is non-empty before publishing.
