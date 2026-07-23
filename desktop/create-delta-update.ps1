param(
    [Parameter(Mandatory = $true)]
    [string]$BaseVersion,
    [Parameter(Mandatory = $true)]
    [string]$Version,
    [Parameter(Mandatory = $true)]
    [string]$BaseWebPackage,
    [Parameter(Mandatory = $true)]
    [string]$WebDirectory,
    [string]$OutputDirectory = ""
)

$ErrorActionPreference = "Stop"

foreach ($candidateVersion in @($BaseVersion, $Version)) {
    if ($candidateVersion -notmatch '^\d+\.\d+\.\d+$') {
        throw "Versions must use X.Y.Z format"
    }
}

$desktopDirectory = $PSScriptRoot
$baseWebPackagePath = [IO.Path]::GetFullPath($BaseWebPackage)
$targetWebDirectory = [IO.Path]::GetFullPath($WebDirectory)
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $releaseDirectory = Join-Path $desktopDirectory "release\v$Version"
} else {
    $releaseDirectory = [IO.Path]::GetFullPath($OutputDirectory)
}

if (-not (Test-Path -LiteralPath $baseWebPackagePath -PathType Leaf)) {
    throw "Base web package was not found: $baseWebPackagePath"
}
if (-not (Test-Path -LiteralPath $targetWebDirectory -PathType Container)) {
    throw "Target web directory was not found: $targetWebDirectory"
}

function Get-ContentVersion([string]$Directory) {
    $versionPath = Join-Path $Directory "version.json"
    if (-not (Test-Path -LiteralPath $versionPath -PathType Leaf)) {
        throw "Version file was not found: $versionPath"
    }
    return (Get-Content -Raw -Encoding UTF8 -LiteralPath $versionPath |
        ConvertFrom-Json).version
}

function Get-RelativeFilePath([string]$Root, [string]$FilePath) {
    $rootPath = [IO.Path]::GetFullPath($Root).TrimEnd(
        [IO.Path]::DirectorySeparatorChar
    ) + [IO.Path]::DirectorySeparatorChar
    $fullPath = [IO.Path]::GetFullPath($FilePath)
    if (-not $fullPath.StartsWith(
        $rootPath,
        [StringComparison]::OrdinalIgnoreCase
    )) {
        throw "File is outside the expected root: $fullPath"
    }
    return $fullPath.Substring($rootPath.Length).Replace('\', '/')
}

function Copy-RelativeFile(
    [string]$SourceRoot,
    [string]$DestinationRoot,
    [string]$RelativePath
) {
    $platformPath = $RelativePath.Replace(
        '/',
        [IO.Path]::DirectorySeparatorChar
    )
    $sourcePath = Join-Path $SourceRoot $platformPath
    $destinationPath = Join-Path $DestinationRoot $platformPath
    $destinationParent = Split-Path $destinationPath -Parent
    New-Item -ItemType Directory -Path $destinationParent -Force |
        Out-Null
    Copy-Item -LiteralPath $sourcePath -Destination $destinationPath -Force
}

$targetContentVersion = Get-ContentVersion $targetWebDirectory
if ($targetContentVersion -ne $Version) {
    throw "Target web version ($targetContentVersion) does not match $Version"
}

New-Item -ItemType Directory -Path $releaseDirectory -Force | Out-Null
$workDirectory = Join-Path $releaseDirectory (
    ".delta-work-" + [Guid]::NewGuid().ToString("N")
)
$baseExtractDirectory = Join-Path $workDirectory "base"
$packageDirectory = Join-Path $workDirectory "package"
$patchWebDirectory = Join-Path $packageDirectory "web"
$verifyWebDirectory = Join-Path $workDirectory "verify\web"

try {
    New-Item -ItemType Directory -Path $baseExtractDirectory -Force |
        Out-Null
    New-Item -ItemType Directory -Path $patchWebDirectory -Force |
        Out-Null
    Expand-Archive -LiteralPath $baseWebPackagePath `
        -DestinationPath $baseExtractDirectory -Force

    $baseWebDirectory = Join-Path $baseExtractDirectory "web"
    $baseContentVersion = Get-ContentVersion $baseWebDirectory
    if ($baseContentVersion -ne $BaseVersion) {
        throw "Base web version ($baseContentVersion) does not match $BaseVersion"
    }

    $targetFiles = Get-ChildItem -LiteralPath $targetWebDirectory `
        -Recurse -File | Sort-Object FullName
    $baseFilesByRelativePath = @{}
    foreach ($baseFile in Get-ChildItem -LiteralPath $baseWebDirectory `
        -Recurse -File) {
        $relativePath = Get-RelativeFilePath $baseWebDirectory $baseFile.FullName
        $baseFilesByRelativePath[$relativePath] = $baseFile.FullName
    }

    $changedFiles = New-Object System.Collections.Generic.List[string]
    $targetPaths = @{}
    $targetManifestLines = New-Object System.Collections.Generic.List[string]
    foreach ($targetFile in $targetFiles) {
        $relativePath = Get-RelativeFilePath `
            $targetWebDirectory `
            $targetFile.FullName
        $targetPaths[$relativePath] = $true
        $targetHash = (Get-FileHash -Algorithm SHA256 `
            -LiteralPath $targetFile.FullName).Hash
        $targetManifestLines.Add("$targetHash $relativePath")

        $isChanged = -not $baseFilesByRelativePath.ContainsKey($relativePath)
        if (-not $isChanged) {
            $basePath = $baseFilesByRelativePath[$relativePath]
            if ((Get-Item -LiteralPath $basePath).Length -ne $targetFile.Length) {
                $isChanged = $true
            } else {
                $baseHash = (Get-FileHash -Algorithm SHA256 `
                    -LiteralPath $basePath).Hash
                $isChanged = $baseHash -ne $targetHash
            }
        }
        if ($isChanged) {
            Copy-RelativeFile `
                $targetWebDirectory `
                $patchWebDirectory `
                $relativePath
            $changedFiles.Add($relativePath)
        }
    }

    $deletedFiles = New-Object System.Collections.Generic.List[string]
    foreach ($relativePath in $baseFilesByRelativePath.Keys) {
        if (-not $targetPaths.ContainsKey($relativePath)) {
            $deletedFiles.Add($relativePath)
        }
    }
    $deletedFiles.Sort()

    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    $patchInfo = [ordered]@{
        TargetVersion = $Version
        SupportedBaseVersions = @($BaseVersion)
    } | ConvertTo-Json -Depth 4
    [IO.File]::WriteAllText(
        (Join-Path $packageDirectory "patch-info.json"),
        $patchInfo,
        $utf8NoBom
    )
    [IO.File]::WriteAllLines(
        (Join-Path $packageDirectory "target-files.sha256"),
        $targetManifestLines,
        $utf8NoBom
    )
    [IO.File]::WriteAllLines(
        (Join-Path $packageDirectory "deleted-files.txt"),
        $deletedFiles,
        $utf8NoBom
    )

    $buildDirectory = Join-Path $desktopDirectory "build"
    & (Join-Path $desktopDirectory "build.ps1") `
        -OutputDirectory $buildDirectory
    if ($LASTEXITCODE -ne 0) {
        throw "Desktop build failed"
    }
    Copy-Item -LiteralPath (
        Join-Path $buildDirectory "MK3 MoveBook Update.exe"
    ) -Destination $packageDirectory -Force
    $payloadDirectory = Join-Path $packageDirectory "payload"
    New-Item -ItemType Directory -Path $payloadDirectory -Force |
        Out-Null
    Copy-Item -LiteralPath (
        Join-Path $buildDirectory "MK3 MoveBook.exe"
    ) -Destination $payloadDirectory -Force

    $readme = @"
MK3 MoveBook — лёгкое обновление $BaseVersion → $Version

1. Закройте MK3 MoveBook.
2. Распакуйте этот архив в отдельную папку.
3. Запустите «MK3 MoveBook Update.exe».
4. Если программа попросит, выберите папку установленного MK3 MoveBook.

Обновление использует уже установленные ролики и добавляет только новые файлы.
"@
    [IO.File]::WriteAllText(
        (Join-Path $packageDirectory "README-update.txt"),
        $readme.Trim() + [Environment]::NewLine,
        $utf8NoBom
    )

    New-Item -ItemType Directory -Path $verifyWebDirectory -Force |
        Out-Null
    foreach ($relativePath in $baseFilesByRelativePath.Keys) {
        Copy-RelativeFile `
            $baseWebDirectory `
            $verifyWebDirectory `
            $relativePath
    }
    foreach ($relativePath in $changedFiles) {
        Copy-RelativeFile `
            $targetWebDirectory `
            $verifyWebDirectory `
            $relativePath
    }
    foreach ($relativePath in $deletedFiles) {
        $platformPath = $relativePath.Replace(
            '/',
            [IO.Path]::DirectorySeparatorChar
        )
        $deletePath = Join-Path $verifyWebDirectory $platformPath
        if (Test-Path -LiteralPath $deletePath -PathType Leaf) {
            Remove-Item -Force -LiteralPath $deletePath
        }
    }

    $verifiedFiles = Get-ChildItem -LiteralPath $verifyWebDirectory `
        -Recurse -File
    if ($verifiedFiles.Count -ne $targetFiles.Count) {
        throw "Delta verification failed: file count differs"
    }
    foreach ($targetFile in $targetFiles) {
        $relativePath = Get-RelativeFilePath `
            $targetWebDirectory `
            $targetFile.FullName
        $platformPath = $relativePath.Replace(
            '/',
            [IO.Path]::DirectorySeparatorChar
        )
        $verifiedPath = Join-Path $verifyWebDirectory $platformPath
        if (-not (Test-Path -LiteralPath $verifiedPath -PathType Leaf) -or
            (Get-FileHash -Algorithm SHA256 -LiteralPath $verifiedPath).Hash -ne
            (Get-FileHash -Algorithm SHA256 -LiteralPath $targetFile.FullName).Hash) {
            throw "Delta verification failed: $relativePath"
        }
    }

    $packageName = "MK3MoveBook-update-$BaseVersion-to-$Version.zip"
    $packagePath = Join-Path $releaseDirectory $packageName
    if (Test-Path -LiteralPath $packagePath) {
        Remove-Item -Force -LiteralPath $packagePath
    }
    Compress-Archive -Path (Join-Path $packageDirectory "*") `
        -DestinationPath $packagePath -CompressionLevel Optimal

    $packageHash = (Get-FileHash -Algorithm SHA256 `
        -LiteralPath $packagePath).Hash
    $manifestPath = Join-Path $releaseDirectory "update-manifest.json"
    if (Test-Path -LiteralPath $manifestPath -PathType Leaf) {
        $manifest = Get-Content -Raw -Encoding UTF8 `
            -LiteralPath $manifestPath | ConvertFrom-Json
        $manifest | Add-Member -Force -NotePropertyName "deltas" `
            -NotePropertyValue @([ordered]@{
                fromVersion = $BaseVersion
                packageUrl = "https://github.com/RinatBy/MK3MoveBook/releases/download/v$Version/$packageName"
                sha256 = $packageHash
            })
        [IO.File]::WriteAllText(
            $manifestPath,
            ($manifest | ConvertTo-Json -Depth 6),
            $utf8NoBom
        )
    }

    Write-Host "Delta update is ready: $packagePath"
    Write-Host "Changed or new files: $($changedFiles.Count)"
    Write-Host "Deleted files: $($deletedFiles.Count)"
} finally {
    if (Test-Path -LiteralPath $workDirectory) {
        Remove-Item -Recurse -Force -LiteralPath $workDirectory
    }
}
