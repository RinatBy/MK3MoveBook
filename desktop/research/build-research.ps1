param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot "build")
)

$ErrorActionPreference = "Stop"

$frameworkDirectory = "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319"
$compiler = Join-Path $frameworkDirectory "csc.exe"
$sources = @(
    (Join-Path $PSScriptRoot "MameResearchCapture.cs"),
    (Join-Path (Split-Path $PSScriptRoot -Parent) "MameFightTracker.cs")
)
$executable = Join-Path $OutputDirectory "MK3 Memory Capture.exe"

foreach ($file in @($compiler) + $sources) {
    if (-not (Test-Path -LiteralPath $file)) {
        throw "Required file was not found: $file"
    }
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

& $compiler `
    /nologo `
    /target:exe `
    /platform:x64 `
    /optimize+ `
    "/out:$executable" `
    "/reference:$(Join-Path $frameworkDirectory 'System.dll')" `
    "/reference:$(Join-Path $frameworkDirectory 'System.Core.dll')" `
    "/reference:$(Join-Path $frameworkDirectory 'System.IO.Compression.dll')" `
    $sources

if ($LASTEXITCODE -ne 0) {
    throw "Compiler exited with code $LASTEXITCODE"
}

Write-Host "Built: $executable"
