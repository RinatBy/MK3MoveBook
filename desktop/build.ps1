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
$authClientSource = Join-Path $desktopDirectory "MoveBookAuthClient.cs"
$sessionStoreSource = Join-Path $desktopDirectory "MoveBookSessionStore.cs"
$achievementQueueStoreSource = Join-Path $desktopDirectory "MoveBookAchievementQueueStore.cs"
$mameBridgeSource = Join-Path $desktopDirectory "MameBridge.cs"
$mameMatchModeClassifierSource = Join-Path $desktopDirectory "MameMatchModeClassifier.cs"
$mameMatchModeTrackerSource = Join-Path $desktopDirectory "MameMatchModeTracker.cs"
$mameAchievementAttributionSource = Join-Path $desktopDirectory "MameAchievementAttribution.cs"
$mameAchievementEligibilityPolicySource = Join-Path $desktopDirectory "MameAchievementEligibilityPolicy.cs"
$mameTeamPlayerAttributionSource = Join-Path $desktopDirectory "MameTeamPlayerAttribution.cs"
$mameDiagnosticReportSource = Join-Path $desktopDirectory "MameDiagnosticReport.cs"
$achievementDiagnosticReportSource = Join-Path $desktopDirectory "MoveBookAchievementDiagnosticReport.cs"
$mameFightTrackerSource = Join-Path $desktopDirectory "MameFightTracker.cs"
$mameFinisherTrackerSource = Join-Path $desktopDirectory "MameFinisherTracker.cs"
$mameSpecialTrackerSource = Join-Path $desktopDirectory "MameSpecialTracker.cs"
$mameCheatMonitorSource = Join-Path $desktopDirectory "MameCheatMonitor.cs"
$mameCheatEngineReaderSource = Join-Path $desktopDirectory "MameCheatEngineReader.cs"
$mameCpuCheatPolicySource = Join-Path $desktopDirectory "MameCpuCheatPolicy.cs"
$webLauncherSource = Join-Path $desktopDirectory "MoveBookWebLauncher.cs"
$deltaUpdaterSource = Join-Path $desktopDirectory "MoveBookDeltaUpdater.cs"
$icon = Join-Path $sourceDirectory "web\assets\icon.ico"
$webIcon = Join-Path $sourceDirectory "web\assets\iconweb.ico"
$executable = Join-Path $OutputDirectory "MK3 MoveBook.exe"
$webLauncherExecutable = Join-Path $OutputDirectory "Web MK3 MBook.exe"
$deltaUpdaterExecutable = Join-Path $OutputDirectory "MK3 MoveBook Update.exe"

$requiredFiles = @(
    $compiler,
    $coreAssembly,
    $formsAssembly,
    $loader,
    $bootstrapper,
    $source,
    $authClientSource,
    $sessionStoreSource,
    $achievementQueueStoreSource,
    $mameBridgeSource,
    $mameMatchModeClassifierSource,
    $mameMatchModeTrackerSource,
    $mameAchievementAttributionSource,
    $mameAchievementEligibilityPolicySource,
    $mameTeamPlayerAttributionSource,
    $mameDiagnosticReportSource,
    $achievementDiagnosticReportSource,
    $mameFightTrackerSource,
    $mameFinisherTrackerSource,
    $mameSpecialTrackerSource,
    $mameCheatMonitorSource,
    $mameCheatEngineReaderSource,
    $mameCpuCheatPolicySource,
    $webLauncherSource,
    $deltaUpdaterSource,
    $icon,
    $webIcon
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
    "/reference:$(Join-Path $frameworkDirectory 'System.Net.Http.dll')" `
    "/reference:$(Join-Path $frameworkDirectory 'System.Security.dll')" `
    "/reference:$(Join-Path $frameworkDirectory 'System.Web.Extensions.dll')" `
    "/reference:$(Join-Path $frameworkDirectory 'System.Windows.Forms.dll')" `
    "/reference:$coreAssembly" `
    "/reference:$formsAssembly" `
    $source `
    $authClientSource `
    $sessionStoreSource `
    $achievementQueueStoreSource `
    $mameBridgeSource `
    $mameMatchModeClassifierSource `
    $mameMatchModeTrackerSource `
    $mameAchievementAttributionSource `
    $mameAchievementEligibilityPolicySource `
    $mameTeamPlayerAttributionSource `
    $mameDiagnosticReportSource `
    $achievementDiagnosticReportSource `
    $mameFightTrackerSource `
    $mameFinisherTrackerSource `
    $mameSpecialTrackerSource `
    $mameCheatMonitorSource `
    $mameCheatEngineReaderSource `
    $mameCpuCheatPolicySource

if ($LASTEXITCODE -ne 0) {
    throw "Компилятор завершился с кодом $LASTEXITCODE"
}

& $compiler `
    /nologo `
    /target:winexe `
    /platform:anycpu `
    /optimize+ `
    "/win32icon:$webIcon" `
    "/out:$webLauncherExecutable" `
    "/reference:$(Join-Path $frameworkDirectory 'System.dll')" `
    "/reference:$(Join-Path $frameworkDirectory 'System.Windows.Forms.dll')" `
    $webLauncherSource

if ($LASTEXITCODE -ne 0) {
    throw "Компилятор веб-запуска завершился с кодом $LASTEXITCODE"
}

& $compiler `
    /nologo `
    /target:winexe `
    /platform:x64 `
    /optimize+ `
    "/win32icon:$icon" `
    "/out:$deltaUpdaterExecutable" `
    "/reference:$(Join-Path $frameworkDirectory 'System.dll')" `
    "/reference:$(Join-Path $frameworkDirectory 'System.Core.dll')" `
    "/reference:$(Join-Path $frameworkDirectory 'System.Web.Extensions.dll')" `
    "/reference:$(Join-Path $frameworkDirectory 'System.Windows.Forms.dll')" `
    $deltaUpdaterSource

if ($LASTEXITCODE -ne 0) {
    throw "Компилятор лёгкого обновления завершился с кодом $LASTEXITCODE"
}

Copy-Item -LiteralPath $coreAssembly -Destination $OutputDirectory -Force
Copy-Item -LiteralPath $formsAssembly -Destination $OutputDirectory -Force
Copy-Item -LiteralPath $loader -Destination $OutputDirectory -Force
Copy-Item -LiteralPath $bootstrapper -Destination $OutputDirectory -Force

Write-Host "Готово: $executable"
Write-Host "Готово: $webLauncherExecutable"
Write-Host "Готово: $deltaUpdaterExecutable"
