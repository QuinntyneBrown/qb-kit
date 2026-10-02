# qb-kit

`qb-kit` is a .NET tool for creating opinionated Angular workspaces.

## Prerequisites

- .NET 10 SDK to build or pack the tool; .NET 10 runtime to run an installed tool.
- Node.js 22.22.3+, 24.15+, or 26+, with npm on `PATH`.

## Build and install locally

Run the installer for your shell from any working directory:

```powershell
./eng/scripts/install-cli.ps1
```

```bat
eng\scripts\install-cli.bat
```

```sh
sh eng/scripts/install-cli.sh
```

Each script packs the current source, installs it into this repository's `.tools` directory, replaces an earlier local install, and checks the installed command's version. The scripts leave any globally installed `qb-kit` unchanged. Run the installed command as `./.tools/qb-kit` on macOS or Linux, or `.\.tools\qb-kit.exe` in PowerShell.

To run the repository tests separately:

```sh
dotnet test QbKit.slnx
```

## Create a workspace

```sh
./.tools/qb-kit new my-workspace
cd my-workspace
npm start
```

Running `qb-kit new` without a name prompts in an interactive terminal. Names use lowercase kebab-case. The command creates one Angular application in `projects/<name>/`, then installs npm dependencies, formats, builds, lints, checks formatting, and runs Jest. It reports success only after all checks pass.

The generated application's default view is a single `Counter` component (`projects/<name>/src/app/counter/`) that increments and decrements a number via two buttons. Its styling is responsive at all form factors (fluid typography and spacing, a wrapping button layout, and touch-sized targets) and is driven by shared CSS custom-property tokens defined in `projects/<name>/src/styles.scss` (colors, spacing, radii, and font sizes), including a `prefers-color-scheme: dark` variant.

The generated workspace offers `npm run build`, `npm run lint`, `npm run format`, `npm run format:check`, `npm test`, and `npm run test:watch`. A failed generation leaves the partial workspace in place for inspection; it never overwrites an existing destination.

The selected Angular and companion versions are bundled with each `qb-kit` release. Update them together after confirming compatibility with a generated workspace.
