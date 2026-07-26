param(
    [string]$BaseUrl = "http://127.0.0.1:8010"
)

$ErrorActionPreference = "Stop"
$apiBase = $BaseUrl.TrimEnd("/")

$loginSession = Invoke-RestMethod `
    -Method Post `
    -Uri "$apiBase/api/v1/auth/device-sessions"

Write-Host "Opening Discord in the system browser..."
Start-Process -FilePath $loginSession.verification_uri

$deadline = [DateTime]::UtcNow.AddSeconds([int]$loginSession.expires_in)
$tokenResult = $null

while ([DateTime]::UtcNow -lt $deadline) {
    Start-Sleep -Seconds ([int]$loginSession.interval)
    $pollResult = Invoke-RestMethod `
        -Method Post `
        -Uri "$apiBase/api/v1/auth/device-sessions/token" `
        -ContentType "application/json" `
        -Body (@{ device_code = $loginSession.device_code } | ConvertTo-Json)

    if ($pollResult.status -eq "authorized") {
        $tokenResult = $pollResult
        break
    }
}

if ($null -eq $tokenResult) {
    throw "Discord login did not complete before the device session expired."
}

$profile = Invoke-RestMethod `
    -Method Get `
    -Uri "$apiBase/api/v1/auth/me" `
    -Headers @{ Authorization = "Bearer $($tokenResult.access_token)" }

Invoke-RestMethod `
    -Method Post `
    -Uri "$apiBase/api/v1/auth/logout" `
    -ContentType "application/json" `
    -Body (@{ refresh_token = $tokenResult.refresh_token } | ConvertTo-Json) `
    | Out-Null

[PSCustomObject]@{
    ok = $true
    user_id = $profile.id
    display_name = $profile.display_name
    providers = ($profile.providers -join ", ")
}
