param(
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"

if ($Version -notmatch '^\d+\.\d+\.\d+$') {
    throw "Version must use X.Y.Z format"
}

$desktopDirectory = $PSScriptRoot
$sourceDirectory = Split-Path $desktopDirectory -Parent
$buildDirectory = Join-Path $desktopDirectory "build"
$releaseRoot = Join-Path $desktopDirectory "release"
$versionReleaseDirectory = Join-Path $releaseRoot "v$Version"
$packageDirectory = Join-Path $versionReleaseDirectory "_package"
$webDirectory = Join-Path $sourceDirectory "web"
$versionFile = Join-Path $webDirectory "version.json"

if (-not (Test-Path -LiteralPath $versionFile)) {
    throw "Version file was not found: $versionFile"
}

$contentVersion = (Get-Content -Raw -Encoding UTF8 -LiteralPath $versionFile |
    ConvertFrom-Json).version
if ($contentVersion -ne $Version) {
    throw "Web version ($contentVersion) does not match release version ($Version)"
}

$resolvedReleaseRoot = [IO.Path]::GetFullPath($releaseRoot)
$resolvedVersionDirectory = [IO.Path]::GetFullPath($versionReleaseDirectory)
if (-not $resolvedVersionDirectory.StartsWith(
    $resolvedReleaseRoot + [IO.Path]::DirectorySeparatorChar,
    [StringComparison]::OrdinalIgnoreCase
)) {
    throw "Unsafe release directory path"
}

if (Test-Path -LiteralPath $versionReleaseDirectory) {
    Remove-Item -Recurse -Force -LiteralPath $versionReleaseDirectory
}
New-Item -ItemType Directory -Path $packageDirectory -Force | Out-Null

& (Join-Path $desktopDirectory "build.ps1") -OutputDirectory $buildDirectory
if ($LASTEXITCODE -ne 0) {
    throw "Desktop build failed"
}

$runtimeFiles = @(
    "MK3 MoveBook.exe",
    "Web MK3 MBook.exe",
    "Microsoft.Web.WebView2.Core.dll",
    "Microsoft.Web.WebView2.WinForms.dll",
    "WebView2Loader.dll",
    "MicrosoftEdgeWebview2Setup.exe"
)
foreach ($fileName in $runtimeFiles) {
    Copy-Item -LiteralPath (Join-Path $buildDirectory $fileName) `
        -Destination $packageDirectory -Force
}

Copy-Item -Recurse -LiteralPath $webDirectory `
    -Destination (Join-Path $packageDirectory "web")

$launchGuide = Join-Path $sourceDirectory "README.txt"
if (Test-Path -LiteralPath $launchGuide) {
    Copy-Item -LiteralPath $launchGuide -Destination $packageDirectory -Force
}

$licensesDirectory = Join-Path $desktopDirectory "licenses"
if (Test-Path -LiteralPath $licensesDirectory) {
    Copy-Item -Recurse -LiteralPath $licensesDirectory `
        -Destination (Join-Path $packageDirectory "licenses")
}

$fullPackageName = "MK3MoveBook-$Version-win-x64.zip"
$webPackageName = "MK3MoveBook-web-$Version.zip"
$fullPackagePath = Join-Path $versionReleaseDirectory $fullPackageName
$webPackagePath = Join-Path $versionReleaseDirectory $webPackageName

Compress-Archive -Path (Join-Path $packageDirectory "*") `
    -DestinationPath $fullPackagePath -CompressionLevel Optimal
Compress-Archive -LiteralPath (Join-Path $packageDirectory "web") `
    -DestinationPath $webPackagePath -CompressionLevel Optimal

$webHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $webPackagePath).Hash
$manifest = [ordered]@{
    version = $Version
    packageUrl = "https://github.com/RinatBy/MK3MoveBook/releases/download/v$Version/$webPackageName"
    sha256 = $webHash
}
$manifestPath = Join-Path $versionReleaseDirectory "update-manifest.json"
$manifest | ConvertTo-Json | Set-Content -Encoding UTF8 -LiteralPath $manifestPath

Remove-Item -Recurse -Force -LiteralPath $packageDirectory

Write-Host "Release files are ready:"
Write-Host "  $fullPackagePath"
Write-Host "  $webPackagePath"
Write-Host "  $manifestPath"
