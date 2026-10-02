# Contributing to qb-kit

Thanks for helping improve qb-kit. Issues and pull requests are welcome.

## Before you start

Check the [issue tracker](https://github.com/QuinntyneBrown/qb-kit/issues) for existing discussion. For a substantial change, open an issue describing the problem and intended behavior before investing in an implementation. For a security issue, follow [SECURITY.md](SECURITY.md) instead of posting details publicly.

## Set up

Install the .NET SDK version from [`global.json`](global.json), a supported Node.js version, and npm. The CLI accepts Node.js `22.22.3+`, `24.15+`, or `26+`.

```shell
dotnet restore QbKit.slnx
dotnet build QbKit.slnx
dotnet test QbKit.slnx
```

The test suite generates a real Angular workspace, installs npm packages, and runs its checks. Allow time and network access for that test.

## Make a change

1. Keep each change focused and align behavior with the [requirements](docs/specs/L2.md).
2. For production behavior, work in small slices: write Given–When–Then acceptance criteria, add an acceptance test, run it and confirm the expected failure, implement the slice, then run the relevant regression checks before continuing.
3. Keep features in vertical slices. Put each class, interface, record, and enum in its own file, with folders and namespaces aligned.
4. Add or update user documentation when commands, prerequisites, generated output, or failure behavior change.

Do not add tests that only inspect source layout, naming, or specification text. Tests should prove behavior. Repository-specific guidance is in [`AGENTS.md`](AGENTS.md).

## Pull requests

Explain the user-visible change and why it is needed. Link the related issue or requirement, describe how you verified it, and mention any prerequisites or known limitations. Keep unrelated refactors separate so reviewers can assess the behavior change clearly.

By participating, you agree to the [code of conduct](CODE_OF_CONDUCT.md). For help using the tool, see [SUPPORT.md](SUPPORT.md).
