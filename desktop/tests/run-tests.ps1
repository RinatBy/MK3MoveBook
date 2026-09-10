$ErrorActionPreference = "Stop"

$desktopDirectory = Split-Path $PSScriptRoot -Parent
$frameworkDirectory = "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319"
$compiler = Join-Path $frameworkDirectory "csc.exe"
$outputDirectory = Join-Path $PSScriptRoot "build"
$executable = Join-Path $outputDirectory "MameFinisherTrackerTests.exe"
$authExecutable = Join-Path $outputDirectory "MoveBookAuthClientTests.exe"
$sessionStoreExecutable = Join-Path $outputDirectory "MoveBookSessionStoreTests.exe"
$achievementQueueStoreExecutable = Join-Path `
    $outputDirectory `
    "MoveBookAchievementQueueStoreTests.exe"
$matchModeClassifierExecutable = Join-Path `
    $outputDirectory `
    "MameMatchModeClassifierTests.exe"
$matchModeTrackerExecutable = Join-Path `
    $outputDirectory `
    "MameMatchModeTrackerTests.exe"
$achievementAttributionExecutable = Join-Path `
    $outputDirectory `
    "MameAchievementAttributionTests.exe"
$teamPlayerAttributionExecutable = Join-Path `
    $outputDirectory `
    "MameTeamPlayerAttributionTests.exe"
$specialTrackerExecutable = Join-Path `
    $outputDirectory `
    "MameSpecialTrackerTests.exe"
$fightTrackerExecutable = Join-Path `
    $outputDirectory `
    "MameFightTrackerTests.exe"
$cpuCheatPolicyExecutable = Join-Path `
    $outputDirectory `
    "MameCpuCheatPolicyTests.exe"

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

& $compiler `
    /nologo `
    /target:exe `
    "/out:$sessionStoreExecutable" `
    "/reference:$(Join-Path $frameworkDirectory 'System.Security.dll')" `
    "/reference:$(Join-Path $frameworkDirectory 'System.Web.Extensions.dll')" `
    (Join-Path $desktopDirectory "MoveBookSessionStore.cs") `
    (Join-Path $PSScriptRoot "MoveBookSessionStoreTests.cs")

if ($LASTEXITCODE -ne 0) {
    throw "Компиляция тестов хранилища сессии завершилась с кодом $LASTEXITCODE"
}

& $sessionStoreExecutable
if ($LASTEXITCODE -ne 0) {
    throw "Тесты хранилища сессии завершились с кодом $LASTEXITCODE"
}

& $compiler `
    /nologo `
    /target:exe `
    "/out:$achievementQueueStoreExecutable" `
    "/reference:$(Join-Path $frameworkDirectory 'System.Net.Http.dll')" `
    "/reference:$(Join-Path $frameworkDirectory 'System.Security.dll')" `
    "/reference:$(Join-Path $frameworkDirectory 'System.Web.Extensions.dll')" `
    (Join-Path $desktopDirectory "MoveBookAuthClient.cs") `
    (Join-Path $desktopDirectory "MoveBookAchievementQueueStore.cs") `
    (Join-Path $PSScriptRoot "MoveBookAchievementQueueStoreTests.cs")

if ($LASTEXITCODE -ne 0) {
    throw "Компиляция тестов очереди завершилась с кодом $LASTEXITCODE"
}

& $achievementQueueStoreExecutable
if ($LASTEXITCODE -ne 0) {
    throw "Тесты очереди завершились с кодом $LASTEXITCODE"
}

& $compiler `
    /nologo `
    /target:exe `
    "/out:$matchModeClassifierExecutable" `
    (Join-Path $desktopDirectory "MameMatchModeClassifier.cs") `
    (Join-Path $PSScriptRoot "MameMatchModeClassifierTests.cs")

if ($LASTEXITCODE -ne 0) {
    throw "Компиляция тестов режима матча завершилась с кодом $LASTEXITCODE"
}

& $matchModeClassifierExecutable
if ($LASTEXITCODE -ne 0) {
    throw "Тесты режима матча завершились с кодом $LASTEXITCODE"
}

& $compiler `
    /nologo `
    /target:exe `
    "/out:$matchModeTrackerExecutable" `
    (Join-Path $desktopDirectory "MameFightTracker.cs") `
    (Join-Path $desktopDirectory "MameMatchModeClassifier.cs") `
    (Join-Path $desktopDirectory "MameMatchModeTracker.cs") `
    (Join-Path $PSScriptRoot "MameMatchModeTrackerTests.cs")

if ($LASTEXITCODE -ne 0) {
    throw "Компиляция тестов удержания режима завершилась с кодом $LASTEXITCODE"
}

& $matchModeTrackerExecutable
if ($LASTEXITCODE -ne 0) {
    throw "Тесты удержания режима завершились с кодом $LASTEXITCODE"
}

& $compiler `
    /nologo `
    /target:exe `
    "/out:$achievementAttributionExecutable" `
    "/reference:$(Join-Path $frameworkDirectory 'System.Net.Http.dll')" `
    "/reference:$(Join-Path $frameworkDirectory 'System.Web.Extensions.dll')" `
    (Join-Path $desktopDirectory "MoveBookAuthClient.cs") `
    (Join-Path $desktopDirectory "MameTeamPlayerAttribution.cs") `
    (Join-Path $desktopDirectory "MameAchievementEligibilityPolicy.cs") `
    (Join-Path $desktopDirectory "MameAchievementAttribution.cs") `
    (Join-Path $PSScriptRoot "MameAchievementAttributionTests.cs")

if ($LASTEXITCODE -ne 0) {
    throw "Achievement-attribution test compilation failed with code $LASTEXITCODE"
}

& $achievementAttributionExecutable
if ($LASTEXITCODE -ne 0) {
    throw "Achievement-attribution tests failed with code $LASTEXITCODE"
}

& $compiler `
    /nologo `
    /target:exe `
    "/out:$teamPlayerAttributionExecutable" `
    (Join-Path $desktopDirectory "MameTeamPlayerAttribution.cs") `
    (Join-Path $PSScriptRoot "MameTeamPlayerAttributionTests.cs")

if ($LASTEXITCODE -ne 0) {
    throw "Team-player-attribution test compilation failed with code $LASTEXITCODE"
}

& $teamPlayerAttributionExecutable
if ($LASTEXITCODE -ne 0) {
    throw "Team-player-attribution tests failed with code $LASTEXITCODE"
}

& $compiler `
    /nologo `
    /target:exe `
    "/out:$specialTrackerExecutable" `
    (Join-Path $desktopDirectory "MameSpecialTracker.cs") `
    (Join-Path $PSScriptRoot "MameSpecialTrackerTests.cs")

if ($LASTEXITCODE -ne 0) {
    throw "Special-tracker test compilation failed with code $LASTEXITCODE"
}

& $specialTrackerExecutable
if ($LASTEXITCODE -ne 0) {
    throw "Special-tracker tests failed with code $LASTEXITCODE"
}

& $compiler `
    /nologo `
    /target:exe `
    "/out:$fightTrackerExecutable" `
    (Join-Path $desktopDirectory "MameFightTracker.cs") `
    (Join-Path $PSScriptRoot "MameFightTrackerTests.cs")

if ($LASTEXITCODE -ne 0) {
    throw "Fight-tracker test compilation failed with code $LASTEXITCODE"
}

& $fightTrackerExecutable
if ($LASTEXITCODE -ne 0) {
    throw "Fight-tracker tests failed with code $LASTEXITCODE"
}

& $compiler `
    /nologo `
    /target:exe `
    /platform:x64 `
    "/out:$cpuCheatPolicyExecutable" `
    (Join-Path $desktopDirectory "MameCheatMonitor.cs") `
    (Join-Path $desktopDirectory "MameCheatEngineReader.cs") `
    (Join-Path $desktopDirectory "MameCpuCheatPolicy.cs") `
    (Join-Path $PSScriptRoot "MameCpuCheatPolicyTests.cs")

if ($LASTEXITCODE -ne 0) {
    throw "CPU cheat-policy test compilation failed with code $LASTEXITCODE"
}

& $cpuCheatPolicyExecutable
if ($LASTEXITCODE -ne 0) {
    throw "CPU cheat-policy tests failed with code $LASTEXITCODE"
}

$romCompatibilityExecutable = Join-Path $outputDirectory "MameRomCompatibilityTests.exe"
& $compiler `
    /nologo /target:exe /platform:x64 `
    "/out:$romCompatibilityExecutable" `
    "/reference:$(Join-Path $frameworkDirectory 'System.Net.Http.dll')" `
    "/reference:$(Join-Path $frameworkDirectory 'System.Web.Extensions.dll')" `
    (Join-Path $desktopDirectory "MameBridge.cs") `
    (Join-Path $desktopDirectory "MameFightTracker.cs") `
    (Join-Path $desktopDirectory "MameFinisherTracker.cs") `
    (Join-Path $desktopDirectory "MameSpecialTracker.cs") `
    (Join-Path $desktopDirectory "MameMatchModeClassifier.cs") `
    (Join-Path $desktopDirectory "MameMatchModeTracker.cs") `
    (Join-Path $desktopDirectory "MameTeamPlayerAttribution.cs") `
    (Join-Path $desktopDirectory "MameAchievementAttribution.cs") `
    (Join-Path $desktopDirectory "MameAchievementEligibilityPolicy.cs") `
    (Join-Path $desktopDirectory "MameCheatMonitor.cs") `
    (Join-Path $desktopDirectory "MameCheatEngineReader.cs") `
    (Join-Path $desktopDirectory "MameCpuCheatPolicy.cs") `
    (Join-Path $desktopDirectory "MoveBookAuthClient.cs") `
    (Join-Path $desktopDirectory "MoveBookAchievementDiagnosticReport.cs") `
    (Join-Path $PSScriptRoot "MameRomCompatibilityTests.cs")
if ($LASTEXITCODE -ne 0) { throw "ROM compatibility test compilation failed: $LASTEXITCODE" }
& $romCompatibilityExecutable (Join-Path $PSScriptRoot "fixtures/umk3tm60-cpu-test17.csv")
if ($LASTEXITCODE -ne 0) { throw "ROM compatibility tests failed: $LASTEXITCODE" }

$engineReaderExecutable = Join-Path $outputDirectory "MameCheatEngineReaderTests.exe"
& $compiler /nologo /target:exe /platform:x64 "/out:$engineReaderExecutable" `
    (Join-Path $desktopDirectory "MameCheatEngineReader.cs") `
    (Join-Path $PSScriptRoot "MameCheatEngineReaderTests.cs")
if ($LASTEXITCODE -ne 0) { throw "Cheat-engine reader test compilation failed: $LASTEXITCODE" }
& $engineReaderExecutable
if ($LASTEXITCODE -ne 0) { throw "Cheat-engine reader tests failed: $LASTEXITCODE" }
