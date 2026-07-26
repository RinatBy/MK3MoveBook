$ErrorActionPreference = "Stop"

$desktopDirectory = Split-Path $PSScriptRoot -Parent
$frameworkDirectory = "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319"
$compiler = Join-Path $frameworkDirectory "csc.exe"
$outputDirectory = Join-Path $PSScriptRoot "build"
$executable = Join-Path $outputDirectory "MameFinisherTrackerTests.exe"
$authExecutable = Join-Path $outputDirectory "MoveBookAuthClientTests.exe"

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

& $compiler `
    /nologo `
    /target:exe `
    "/out:$authExecutable" `
    "/reference:$(Join-Path $frameworkDirectory 'System.Net.Http.dll')" `
    "/reference:$(Join-Path $frameworkDirectory 'System.Web.Extensions.dll')" `
    (Join-Path $desktopDirectory "MoveBookAuthClient.cs") `
    (Join-Path $PSScriptRoot "MoveBookAuthClientTests.cs")

if ($LASTEXITCODE -ne 0) {
    throw "Компиляция OAuth-тестов завершилась с кодом $LASTEXITCODE"
}

& $authExecutable
if ($LASTEXITCODE -ne 0) {
    throw "OAuth-тесты завершились с кодом $LASTEXITCODE"
}
