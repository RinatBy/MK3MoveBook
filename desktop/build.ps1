param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot "build")
)

$ErrorActionPreference = "Stop"

$desktopDirectory = $PSScriptRoot
$sourceDirectory = Split-Path $desktopDirectory -Parent
$webViewDirectory = Join-Path $desktopDirectory "vendor\webview2"
$frameworkDirectory = "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319"
$compiler = Join-Path $frameworkDirectory "csc.exe"
$coreAssembly = Join-Path $webViewDirectory "lib\net462\Microsoft.Web.WebView2.Core.dll"
$formsAssembly = Join-Path $webViewDirectory "lib\net462\Microsoft.Web.WebView2.WinForms.dll"
$loader = Join-Path $webViewDirectory "runtimes\win-x64\native\WebView2Loader.dll"
$bootstrapper = Join-Path $webViewDirectory "MicrosoftEdgeWebview2Setup.exe"
$source = Join-Path $desktopDirectory "MoveBookHost.cs"
$icon = Join-Path $sourceDirectory "web\assets\icon.ico"
$executable = Join-Path $OutputDirectory "MK3 MoveBook.exe"

$requiredFiles = @(
    $compiler,
    $coreAssembly,
    $formsAssembly,
    $loader,
    $bootstrapper,
    $source,
    $icon
)
foreach ($file in $requiredFiles) {
    if (-not (Test-Path -LiteralPath $file)) {
        throw "Не найден обязательный файл: $file"
    }
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

& $compiler `
    /nologo `
    /target:winexe `
    /platform:x64 `
    /optimize+ `
    "/win32icon:$icon" `
    "/out:$executable" `
    "/reference:$(Join-Path $frameworkDirectory 'System.dll')" `
    "/reference:$(Join-Path $frameworkDirectory 'System.Core.dll')" `
    "/reference:$(Join-Path $frameworkDirectory 'System.Drawing.dll')" `
    "/reference:$(Join-Path $frameworkDirectory 'System.IO.Compression.dll')" `
    "/reference:$(Join-Path $frameworkDirectory 'System.Web.Extensions.dll')" `
    "/reference:$(Join-Path $frameworkDirectory 'System.Windows.Forms.dll')" `
    "/reference:$coreAssembly" `
    "/reference:$formsAssembly" `
    $source

if ($LASTEXITCODE -ne 0) {
    throw "Компилятор завершился с кодом $LASTEXITCODE"
}

Copy-Item -LiteralPath $coreAssembly -Destination $OutputDirectory -Force
Copy-Item -LiteralPath $formsAssembly -Destination $OutputDirectory -Force
Copy-Item -LiteralPath $loader -Destination $OutputDirectory -Force
Copy-Item -LiteralPath $bootstrapper -Destination $OutputDirectory -Force

Write-Host "Готово: $executable"
