# VisualStudioUpdate

This .NET 10 console application generates an Atom feed containing the release updates documented for Visual Studio 2026 and Visual Studio 2026 Insiders.

The application reads the official Microsoft Learn release-note pages instead of the Visual Studio blog RSS feed, because the blog feed does not document every release:

- https://learn.microsoft.com/en-us/visualstudio/releases/2026/release-notes
- https://learn.microsoft.com/en-us/visualstudio/releases/2026/release-notes-insiders

## Usage

```powershell
dotnet run --project .\\VisualStudioUpdateRSS -- --output .\\visual-studio-2026.atom
```

The default output file is `visual-studio-2026.atom`. The generated Atom document can be added directly to an RSS/Atom reader.

## Publish on GitHub

The included `.github/workflows/publish-feed.yml` regenerates the feed every day and deploys it to GitHub Pages.

1. Push the repository to GitHub and open **Settings > Pages**.
2. Set **Source** to **GitHub Actions**.
3. Run the **Publish Visual Studio Atom feed** workflow once from the **Actions** tab.

After the first deployment, subscribe to:

```text
https://YOUR-OWNER.github.io/YOUR-REPOSITORY/visual-studio-2026.atom
```

Replace `YOUR-OWNER` and `YOUR-REPOSITORY` with the GitHub repository owner and name. The scheduled workflow keeps the feed updated automatically.
