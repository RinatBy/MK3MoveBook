param(
    [Parameter(Mandatory = $true)]
    [string]$BaseFullArchive,

    [Parameter(Mandatory = $true)]
    [string]$BaseWebArchive,

    [Parameter(Mandatory = $true)]
    [string]$CurrentWebRoot,

    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory,

    [Parameter(Mandatory = $true)]
    [string]$Version,

    [string]$Repository = "RinatBy/MK3MoveBook"
)

$ErrorActionPreference = "Stop"

if ($Version -notmatch '^\d+\.\d+\.\d+$') {
    throw "Version must use X.Y.Z format"
}
if ($Repository -notmatch '^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$') {
    throw "Repository must use owner/name format"
}

$baseFullPath = (Resolve-Path -LiteralPath $BaseFullArchive).Path
$baseWebPath = (Resolve-Path -LiteralPath $BaseWebArchive).Path
$currentWebPath = (Resolve-Path -LiteralPath $CurrentWebRoot).Path
$sourceVersionPath = Join-Path $currentWebPath "version.json"

if (-not (Test-Path -LiteralPath $sourceVersionPath)) {
    throw "Current web version file was not found: $sourceVersionPath"
}

$sourceVersion = (Get-Content -Raw -Encoding UTF8 -LiteralPath $sourceVersionPath |
    ConvertFrom-Json).version
if ($sourceVersion -ne $Version) {
    throw "Current web version ($sourceVersion) does not match release version ($Version)"
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$outputPath = (Resolve-Path -LiteralPath $OutputDirectory).Path
$workPath = Join-Path ([IO.Path]::GetTempPath()) (
    "mk3-release-" + [Guid]::NewGuid().ToString("N")
)
$fullPackageRoot = Join-Path $workPath "full"
$webPackageRoot = Join-Path $workPath "web"

function Get-VideoManifest {
    param([Parameter(Mandatory = $true)][string]$WebRoot)

    $movesRoot = Join-Path $WebRoot "assets/moves"
    if (-not (Test-Path -LiteralPath $movesRoot)) {
        throw "Video directory was not found: $movesRoot"
    }

    $resolvedMovesRoot = (Resolve-Path -LiteralPath $movesRoot).Path.TrimEnd('\', '/')
    $files = @(Get-ChildItem -LiteralPath $resolvedMovesRoot -Recurse -File -Filter "*.webm")
    if ($files.Count -eq 0) {
        throw "The base archive does not contain WEBM videos: $movesRoot"
    }

    return @($files | ForEach-Object {
        $relativePath = $_.FullName.Substring($resolvedMovesRoot.Length).TrimStart('\', '/')
        ($relativePath -replace '\\', '/') + "|" + $_.Length
    } | Sort-Object)
}

function Assert-ManifestsEqual {
    param(
        [Parameter(Mandatory = $true)][string[]]$Expected,
        [Parameter(Mandatory = $true)][string[]]$Actual,
        [Parameter(Mandatory = $true)][string]$Label
    )

    $difference = @(Compare-Object -ReferenceObject $Expected -DifferenceObject $Actual)
    if ($difference.Count -ne 0) {
        $preview = ($difference | Select-Object -First 10 | Out-String).Trim()
        throw "Video manifest changed while building $Label.`n$preview"
    }
}

function Copy-CurrentWebFiles {
    param([Parameter(Mandatory = $true)][string]$DestinationWebRoot)

    $sourceRoot = $currentWebPath.TrimEnd('\', '/')
    $excludedRoot = [IO.Path]::GetFullPath(
        (Join-Path $sourceRoot "assets/moves")
    ).TrimEnd('\', '/')
    $copied = 0

    Get-ChildItem -LiteralPath $sourceRoot -Recurse -File | ForEach-Object {
        $sourceFile = [IO.Path]::GetFullPath($_.FullName)
        $isVideoAsset = $sourceFile.Equals(
            $excludedRoot,
            [StringComparison]::OrdinalIgnoreCase
        ) -or $sourceFile.StartsWith(
            $excludedRoot + [IO.Path]::DirectorySeparatorChar,
            [StringComparison]::OrdinalIgnoreCase
        )
        if ($isVideoAsset) {
            return
        }

        $relativePath = $sourceFile.Substring($sourceRoot.Length).TrimStart('\', '/')
        $targetFile = Join-Path $DestinationWebRoot $relativePath
        $targetDirectory = Split-Path $targetFile -Parent
        New-Item -ItemType Directory -Path $targetDirectory -Force | Out-Null
        Copy-Item -LiteralPath $sourceFile -Destination $targetFile -Force
        $copied += 1
    }

    if ($copied -eq 0) {
        throw "No current web files were copied to $DestinationWebRoot"
    }
}

function Get-ArchiveVideoManifest {
    param([Parameter(Mandatory = $true)][string]$ArchivePath)

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [IO.Compression.ZipFile]::OpenRead($ArchivePath)
    try {
        $entries = @($archive.Entries | Where-Object {
            ($_.FullName -replace '\\', '/') -match '^web/assets/moves/.+\.webm$'
        })
        if ($entries.Count -eq 0) {
            throw "Built archive does not contain WEBM videos: $ArchivePath"
        }

        return @($entries | ForEach-Object {
            $path = $_.FullName -replace '\\', '/'
            $relativePath = $path.Substring("web/assets/moves/".Length)
            $relativePath + "|" + $_.Length
        } | Sort-Object)
    }
    finally {
        $archive.Dispose()
    }
}

function Assert-ArchiveVersion {
    param([Parameter(Mandatory = $true)][string]$ArchivePath)

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [IO.Compression.ZipFile]::OpenRead($ArchivePath)
    try {
        $entry = $archive.Entries | Where-Object {
            ($_.FullName -replace '\\', '/') -eq "web/version.json"
        } | Select-Object -First 1
        if ($null -eq $entry) {
            throw "Built archive does not contain web/version.json: $ArchivePath"
        }

        $reader = New-Object IO.StreamReader($entry.Open(), [Text.Encoding]::UTF8)
        try {
            $archiveVersion = ($reader.ReadToEnd() | ConvertFrom-Json).version
        }
        finally {
            $reader.Dispose()
        }
        if ($archiveVersion -ne $Version) {
            throw "Built archive version ($archiveVersion) does not match $Version"
        }
    }
    finally {
        $archive.Dispose()
    }
}

New-Item -ItemType Directory -Path $fullPackageRoot -Force | Out-Null
New-Item -ItemType Directory -Path $webPackageRoot -Force | Out-Null

try {
    Expand-Archive -LiteralPath $baseFullPath -DestinationPath $fullPackageRoot
    Expand-Archive -LiteralPath $baseWebPath -DestinationPath $webPackageRoot

    $fullWebRoot = Join-Path $fullPackageRoot "web"
    $standaloneWebRoot = Join-Path $webPackageRoot "web"
    foreach ($webRoot in @($fullWebRoot, $standaloneWebRoot)) {
        if (-not (Test-Path -LiteralPath (Join-Path $webRoot "index.html"))) {
            throw "Unexpected base archive structure: $webRoot/index.html was not found"
        }
    }

    $fullBaseVideos = @(Get-VideoManifest -WebRoot $fullWebRoot)
    $webBaseVideos = @(Get-VideoManifest -WebRoot $standaloneWebRoot)

    Copy-CurrentWebFiles -DestinationWebRoot $fullWebRoot
    Copy-CurrentWebFiles -DestinationWebRoot $standaloneWebRoot

    Assert-ManifestsEqual `
        -Expected $fullBaseVideos `
        -Actual @(Get-VideoManifest -WebRoot $fullWebRoot) `
        -Label "the full package"
    Assert-ManifestsEqual `
        -Expected $webBaseVideos `
        -Actual @(Get-VideoManifest -WebRoot $standaloneWebRoot) `
        -Label "the web package"

    $fullPackageName = "MK3MoveBook-$Version-win-x64.zip"
    $webPackageName = "MK3MoveBook-web-$Version.zip"
    $fullPackagePath = Join-Path $outputPath $fullPackageName
    $webPackagePath = Join-Path $outputPath $webPackageName
    foreach ($archivePath in @($fullPackagePath, $webPackagePath)) {
        if (Test-Path -LiteralPath $archivePath) {
            Remove-Item -Force -LiteralPath $archivePath
        }
    }

    Compress-Archive -Path (Join-Path $fullPackageRoot "*") `
        -DestinationPath $fullPackagePath -CompressionLevel Optimal
    Compress-Archive -LiteralPath $standaloneWebRoot `
        -DestinationPath $webPackagePath -CompressionLevel Optimal

    Assert-ManifestsEqual `
        -Expected $fullBaseVideos `
        -Actual @(Get-ArchiveVideoManifest -ArchivePath $fullPackagePath) `
        -Label $fullPackageName
    Assert-ManifestsEqual `
        -Expected $webBaseVideos `
        -Actual @(Get-ArchiveVideoManifest -ArchivePath $webPackagePath) `
        -Label $webPackageName
    Assert-ArchiveVersion -ArchivePath $fullPackagePath
    Assert-ArchiveVersion -ArchivePath $webPackagePath

    $webHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $webPackagePath).Hash
    $manifest = [ordered]@{
        version = $Version
        packageUrl = "https://github.com/$Repository/releases/download/v$Version/$webPackageName"
        sha256 = $webHash
    }
    $manifestPath = Join-Path $outputPath "update-manifest.json"
    $manifest | ConvertTo-Json | Set-Content -Encoding UTF8 -LiteralPath $manifestPath

    [pscustomobject]@{
        Version = $Version
        FullArchive = $fullPackagePath
        WebArchive = $webPackagePath
        Manifest = $manifestPath
        FullVideoCount = $fullBaseVideos.Count
        WebVideoCount = $webBaseVideos.Count
    }
}
finally {
    if (Test-Path -LiteralPath $workPath) {
        Remove-Item -Recurse -Force -LiteralPath $workPath
    }
}
