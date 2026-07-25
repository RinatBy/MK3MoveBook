$ErrorActionPreference = "Stop"

$desktopDirectory = Split-Path $PSScriptRoot -Parent
$frameworkDirectory = "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319"
$compiler = Join-Path $frameworkDirectory "csc.exe"
$outputDirectory = Join-Path $PSScriptRoot "build"
$executable = Join-Path $outputDirectory "MameFinisherTrackerTests.exe"

New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null

& $compiler `
    /nologo `
    /target:exe `
    "/out:$executable" `
    (Join-Path $desktopDirectory "MameFinisherTracker.cs") `
    (Join-Path $PSScriptRoot "MameFinisherTrackerTests.cs")

if ($LASTEXITCODE -ne 0) {
    throw "Компиляция тестов завершилась с кодом $LASTEXITCODE"
}

& $executable
if ($LASTEXITCODE -ne 0) {
    throw "Тесты завершились с кодом $LASTEXITCODE"
}
