#!/bin/sh
set -eu

repository=$(CDPATH= cd "$(dirname "$0")/../.." && pwd)
project="$repository/src/QbKit/QbKit.csproj"
tool_path=${QBKIT_TOOL_PATH:-"$repository/.tools"}
temporary=$(mktemp -d "${TMPDIR:-/tmp}/qb-kit-install.XXXXXX")
trap 'rm -rf -- "$temporary"' EXIT HUP INT TERM

version=$(dotnet msbuild "$project" -getProperty:Version)
test -n "$version" || { echo 'Could not read the CLI package version.' >&2; exit 1; }

dotnet pack "$project" --configuration Release --output "$temporary"
test -f "$temporary/qb-kit.$version.nupkg" || { echo 'The qb-kit package was not created.' >&2; exit 1; }

mkdir -p "$tool_path"
command="$tool_path/qb-kit"
if test -e "$command"; then
    dotnet tool uninstall qb-kit --tool-path "$tool_path"
fi

NUGET_PACKAGES="$temporary/packages" dotnet tool install qb-kit \
    --tool-path "$tool_path" --version "$version" --source "$temporary" --no-http-cache
test -x "$command" || { echo "Installed command was not found: $command" >&2; exit 1; }
installed_version=$("$command" --version)
test "${installed_version%%+*}" = "$version" || {
    echo "Installed version '$installed_version' does not match build version '$version'." >&2
    exit 1
}
printf 'Installed qb-kit %s at %s\n' "$version" "$command"
