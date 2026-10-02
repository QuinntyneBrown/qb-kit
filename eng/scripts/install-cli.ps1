$ErrorActionPreference = 'Stop'

$repository = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$project = Join-Path $repository 'src\QbKit\QbKit.csproj'
$toolPath = if ($env:QBKIT_TOOL_PATH) { $env:QBKIT_TOOL_PATH } else { Join-Path $repository '.tools' }
$temporary = Join-Path ([IO.Path]::GetTempPath()) ('qb-kit-install-' + [Guid]::NewGuid().ToString('N'))
$packages = Join-Path $temporary 'packages'
$previousPackages = $env:NUGET_PACKAGES

try {
    [IO.Directory]::CreateDirectory($temporary) | Out-Null
    $version = (& dotnet msbuild $project -getProperty:Version).Trim()
    if ($LASTEXITCODE -ne 0 -or -not $version) { throw 'Could not read the CLI package version.' }

    & dotnet pack $project --configuration Release --output $temporary
    if ($LASTEXITCODE -ne 0) { throw 'Packing qb-kit failed.' }
    $package = Join-Path $temporary "qb-kit.$version.nupkg"
    if (-not (Test-Path -LiteralPath $package)) { throw "Package was not created: $package" }

    [IO.Directory]::CreateDirectory($toolPath) | Out-Null
    $command = Join-Path $toolPath 'qb-kit.exe'
    if (Test-Path -LiteralPath $command) {
        & dotnet tool uninstall qb-kit --tool-path $toolPath
        if ($LASTEXITCODE -ne 0) { throw 'Removing the previous qb-kit installation failed.' }
    }

    $env:NUGET_PACKAGES = $packages
    & dotnet tool install qb-kit --tool-path $toolPath --version $version --source $temporary --no-http-cache
    if ($LASTEXITCODE -ne 0) { throw 'Installing qb-kit failed.' }
    if (-not (Test-Path -LiteralPath $command)) { throw "Installed command was not found: $command" }
    $installedVersion = (& $command --version).Trim()
    if ($LASTEXITCODE -ne 0 -or $installedVersion.Split('+')[0] -ne $version) {
        throw "Installed version '$installedVersion' does not match build version '$version'."
    }
    Write-Host "Installed qb-kit $version at $command"
}
catch {
    [Console]::Error.WriteLine($_)
    exit 1
}
finally {
    $env:NUGET_PACKAGES = $previousPackages
    if (Test-Path -LiteralPath $temporary) { [IO.Directory]::Delete($temporary, $true) }
}
