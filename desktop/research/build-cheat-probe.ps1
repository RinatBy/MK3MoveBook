param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot "build")
)

$ErrorActionPreference = "Stop"

$frameworkDirectory = "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319"
$compiler = Join-Path $frameworkDirectory "csc.exe"
$source = Join-Path $PSScriptRoot "MameCheatStateProbe.cs"
$executable = Join-Path $OutputDirectory "MK3 Cheat State Probe.exe"

foreach ($file in @($compiler, $source)) {
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
    $source

if ($LASTEXITCODE -ne 0) {
    throw "Compiler exited with code $LASTEXITCODE"
}

Write-Host "Built: $executable"
