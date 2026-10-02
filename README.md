# qb-kit

`qb-kit` is a .NET tool for creating opinionated Angular workspaces.

## Prerequisites

- .NET 10 SDK to build or pack the tool; .NET 10 runtime to run an installed tool.
- Node.js 22.22.3+, 24.15+, or 26+, with npm on `PATH`.

## Build and install locally

```sh
dotnet test QbKit.slnx
dotnet pack src/QbKit/QbKit.csproj -c Release -o nupkg
dotnet tool install --global qb-kit --version 0.1.0 --add-source nupkg
```

If a different `qb-kit` package is already installed, remove or update it before installing the local package.

## Create a workspace

```sh
qb-kit new my-workspace
cd my-workspace
npm start
```

Running `qb-kit new` without a name prompts in an interactive terminal. Names use lowercase kebab-case. The command creates one Angular application in `projects/<name>/`, then installs npm dependencies, formats, builds, lints, checks formatting, and runs Jest. It reports success only after all checks pass.

The generated workspace offers `npm run build`, `npm run lint`, `npm run format`, `npm run format:check`, `npm test`, and `npm run test:watch`. A failed generation leaves the partial workspace in place for inspection; it never overwrites an existing destination.

The selected Angular and companion versions are bundled with each `qb-kit` release. Update them together after confirming compatibility with a generated workspace.
