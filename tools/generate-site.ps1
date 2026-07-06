[CmdletBinding()]
param(
    [string]$WebRoot = (Join-Path $PSScriptRoot "..\web"),
    [string]$OutputRoot = (Join-Path $PSScriptRoot "..\.site-dist"),
    [string]$BaseUrl = "https://rinatby.github.io/MK3MoveBook/"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$Utf8NoBom = New-Object System.Text.UTF8Encoding($false)
$TrimChars = [char[]]@("\", "/")
$WebPath = (Resolve-Path -LiteralPath $WebRoot).Path.TrimEnd($TrimChars)
$OutputPath = [IO.Path]::GetFullPath($OutputRoot).TrimEnd($TrimChars)
$DriveRoot = [IO.Path]::GetPathRoot($OutputPath).TrimEnd($TrimChars)

if ($OutputPath -eq $DriveRoot) {
    throw "OutputRoot cannot be a drive root: $OutputPath"
}
if ($OutputPath.Equals($WebPath, [StringComparison]::OrdinalIgnoreCase) -or
    $OutputPath.StartsWith(
        $WebPath + [IO.Path]::DirectorySeparatorChar,
        [StringComparison]::OrdinalIgnoreCase
    )) {
    throw "OutputRoot cannot be WebRoot or a directory inside it."
}

$BaseUri = [Uri]($BaseUrl.TrimEnd("/") + "/")
if ($BaseUri.Scheme -notin @("http", "https")) {
    throw "BaseUrl must use http or https."
}
$NormalizedBaseUrl = $BaseUri.AbsoluteUri
$BaseHref = $BaseUri.AbsolutePath
if (-not $BaseHref.EndsWith("/")) {
    $BaseHref += "/"
}

$MovesPath = Join-Path $WebPath "data\moves.js"
$TemplatePath = Join-Path $WebPath "index.html"
$MovesSource = [IO.File]::ReadAllText($MovesPath, [Text.Encoding]::UTF8)
$MovesJson = $MovesSource `
    -replace "^\s*window\.MOVEBOOK_DATA\s*=\s*", "" `
    -replace ";\s*$", ""
$Data = $MovesJson | ConvertFrom-Json
$Template = [IO.File]::ReadAllText($TemplatePath, [Text.Encoding]::UTF8)
$Fighters = @($Data.versions.umk3uk.fighters)

function Get-PropertyValue {
    param(
        [object]$Object,
        [string]$Name
    )
    if ($null -eq $Object) {
        return $null
    }
    $Property = $Object.PSObject.Properties[$Name]
    if ($null -eq $Property) {
        return $null
    }
    return $Property.Value
}

function ConvertTo-HtmlText {
    param([object]$Value)
    return [Net.WebUtility]::HtmlEncode([string]$Value)
}

function Get-SeoName {
    param([object]$Fighter)
    $Name = Get-PropertyValue -Object $Data.seo.fighters -Name $Fighter.id
    if ($Name) {
        return [string]$Name
    }
    return [string]$Fighter.name
}

function Get-FighterPlatformInfo {
    param(
        [object]$Fighter,
        [string]$Platform
    )
    $Platforms = @($Data.platformModel.fighters.default.platforms)
    $Override = Get-PropertyValue `
        -Object $Data.platformModel.fighters.overrides `
        -Name $Fighter.id
    $OverridePlatforms = Get-PropertyValue -Object $Override -Name "platforms"
    if ($OverridePlatforms) {
        $Platforms = @($OverridePlatforms)
    }
    $Notes = Get-PropertyValue -Object $Override -Name "platformNotes"
    return [pscustomobject]@{
        Available = $Platforms -contains $Platform
        Note = [string](Get-PropertyValue -Object $Notes -Name $Platform)
    }
}

function Convert-Notation {
    param(
        [string]$Notation,
        [string]$Platform
    )
    if ($Platform -ne "sega") {
        return $Notation
    }
    $Result = $Notation
    $Labels = [ordered]@{
        "RUN" = "Y"
        "HP" = "X"
        "LP" = "A"
        "HK" = "Z"
        "LK" = "C"
        "BL" = "B"
    }
    foreach ($Action in $Labels.Keys) {
        $Result = [regex]::Replace(
            $Result,
            "\b$Action\b",
            $Labels[$Action]
        )
    }
    return $Result
}

function New-MoveMarkup {
    param(
        [object]$Fighter,
        [string]$Platform
    )
    $Sections = New-Object System.Collections.Generic.List[string]
    $MoveCount = 0
    foreach ($Category in $Fighter.categories) {
        if ([string]$Category.name -match "(?i)\btrilogy\b") {
            continue
        }
        $MoveRows = New-Object System.Collections.Generic.List[string]
        foreach ($Move in $Category.moves) {
            if ([string]$Move.label -match "(?i)\btrilogy\b") {
                continue
            }
            $MoveCount++
            $Pretty = Get-PropertyValue -Object $Move -Name "pretty"
            if (-not $Pretty) {
                $Pretty = $Move.notation
            }
            $Pretty = Convert-Notation -Notation ([string]$Pretty) -Platform $Platform
            $MoveRows.Add(@"
                            <article class="move-card">
                                <div class="move-name">$(ConvertTo-HtmlText $Move.label)</div>
                                <div class="move-command">
                                    <span class="sequence-text">$(ConvertTo-HtmlText $Pretty)</span>
                                </div>
                            </article>
"@)
        }
        if ($MoveRows.Count) {
            $Sections.Add(@"
                    <section class="chapter">
                        <h2 class="chapter-heading">$(ConvertTo-HtmlText $Category.name)</h2>
                        <div class="move-list">
$($MoveRows -join [Environment]::NewLine)
                        </div>
                    </section>
"@)
        }
    }
    return [pscustomobject]@{
        Html = $Sections -join [Environment]::NewLine
        Count = $MoveCount
    }
}

function New-RosterMarkup {
    param(
        [string]$Platform,
        [string]$SelectedFighterId
    )
    $Cards = foreach ($Fighter in $Fighters) {
        $Info = Get-FighterPlatformInfo -Fighter $Fighter -Platform $Platform
        $ClassName = if ($Info.Available) {
            "fighter-card"
        } else {
            "fighter-card is-platform-unavailable"
        }
        $Badge = if ($Info.Available) {
            ""
        } elseif ($Platform -eq "sega") {
            "<em>НЕТ В SEGA</em>"
        } else {
            "<em>НЕТ В ВЕРСИИ</em>"
        }
        $Selected = if ($Fighter.id -eq $SelectedFighterId) {
            "true"
        } else {
            "false"
        }
        $Number = ([int]$Fighter.number).ToString("00")
@"
                    <a class="$ClassName" role="option"
                        href="$BaseHref$Platform/$($Fighter.id)/"
                        aria-selected="$Selected">
                        <img src="assets/portraits/$($Fighter.id).png" alt="">
                        <span><b>$(ConvertTo-HtmlText $Fighter.name)</b>$Badge</span>
                        <small>$Number</small>
                    </a>
"@
    }
    return $Cards -join [Environment]::NewLine
}

function Get-PageMetadata {
    param(
        [ValidateSet("root", "platform", "fighter")]
        [string]$Kind,
        [string]$Platform,
        [object]$Fighter
    )
    $SiteName = [string]$Data.seo.siteName
    $SeoName = Get-SeoName -Fighter $Fighter
    $PlatformInfo = Get-FighterPlatformInfo -Fighter $Fighter -Platform $Platform
    if ($Kind -eq "root") {
        return [pscustomobject]@{
            Title = "MK3 MoveBook — приёмы и фаталити Ultimate Mortal Kombat 3"
            Description = "Интерактивная книга приёмов Ultimate Mortal Kombat 3 для аркады и SEGA: комбинации кнопок, фаталити, комбо и видеодемонстрации."
        }
    }
    if ($Kind -eq "platform") {
        $PlatformTitle = if ($Platform -eq "sega") {
            "Ultimate Mortal Kombat 3 для SEGA"
        } else {
            "Аркадная Ultimate Mortal Kombat 3"
        }
        return [pscustomobject]@{
            Title = "$PlatformTitle — приёмы и фаталити | $SiteName"
            Description = "Интерактивная книга ${PlatformTitle}: бойцы, комбинации кнопок, фаталити, комбо и видеодемонстрации."
        }
    }
    if (-not $PlatformInfo.Available) {
        return [pscustomobject]@{
            Title = "$SeoName — недоступен в UMK3 на SEGA | $SiteName"
            Description = "$SeoName отсутствует в стандартной Ultimate Mortal Kombat 3 для SEGA. На странице сохранены аркадные приёмы для справки."
        }
    }
    $TitlePlatform = if ($Platform -eq "sega") { "на SEGA" } else { "в аркаде" }
    $Description = if ($Platform -eq "sega") {
        "Справочник персонажа «$SeoName» в Ultimate Mortal Kombat 3 для SEGA: доступность приёмов, комбинации для проверки, фаталити, комбо и видеодемонстрации."
    } else {
        "Все приёмы персонажа «$SeoName» в аркадной Ultimate Mortal Kombat 3: комбинации кнопок, фаталити, комбо и видеодемонстрации."
    }
    return [pscustomobject]@{
        Title = "$SeoName — приёмы и фаталити UMK3 $TitlePlatform | $SiteName"
        Description = $Description
    }
}

function New-StaticPage {
    param(
        [ValidateSet("root", "platform", "fighter")]
        [string]$Kind,
        [string]$Platform,
        [object]$Fighter,
        [string]$CanonicalUrl
    )
    $Metadata = Get-PageMetadata `
        -Kind $Kind `
        -Platform $Platform `
        -Fighter $Fighter
    $MoveMarkup = New-MoveMarkup -Fighter $Fighter -Platform $Platform
    $RosterMarkup = New-RosterMarkup `
        -Platform $Platform `
        -SelectedFighterId $Fighter.id
    $PlatformInfo = Get-FighterPlatformInfo `
        -Fighter $Fighter `
        -Platform $Platform
    $OgImage = "$NormalizedBaseUrl" +
        "assets/portraits/$($Fighter.id).png"
    $OgType = if ($Kind -eq "fighter") { "article" } else { "website" }
    $SeoTags = @"
    <link rel="canonical" href="$(ConvertTo-HtmlText $CanonicalUrl)">
    <meta property="og:type" content="$OgType">
    <meta property="og:site_name" content="$(ConvertTo-HtmlText $Data.seo.siteName)">
    <meta property="og:title" content="$(ConvertTo-HtmlText $Metadata.Title)">
    <meta property="og:description" content="$(ConvertTo-HtmlText $Metadata.Description)">
    <meta property="og:url" content="$(ConvertTo-HtmlText $CanonicalUrl)">
    <meta property="og:image" content="$(ConvertTo-HtmlText $OgImage)">
    <meta name="twitter:card" content="summary">
"@

    $Html = $Template
    $Html = [regex]::Replace(
        $Html,
        '(<a id="portableDownload" class="portable-download"\s+href="[^"]+")\s+hidden>',
        '$1>'
    )
    $Html = [regex]::Replace(
        $Html,
        '<body>',
        '<body class="hosted-site">',
        1
    )
    $Html = $Html -replace "<head>", "<head>`r`n    <base href=`"$BaseHref`">"
    $Html = [regex]::Replace(
        $Html,
        "(?s)<title>.*?</title>",
        "<title>$(ConvertTo-HtmlText $Metadata.Title)</title>"
    )
    $Html = [regex]::Replace(
        $Html,
        "(?s)<meta id=""metaDescription""\s+name=""description""\s+content=""[^""]*"">",
        "<meta id=""metaDescription"" name=""description"" content=""$(ConvertTo-HtmlText $Metadata.Description)"">"
    )
    $Html = $Html -replace "</head>", "$SeoTags`r`n</head>"
    $Html = $Html -replace (
        '<h1 id="fighterTitle"></h1>'
    ), (
        '<h1 id="fighterTitle">' +
        (ConvertTo-HtmlText $Fighter.name) +
        "</h1>"
    )
    $Html = $Html -replace (
        '<img id="heroPortrait" alt="">'
    ), (
        '<img id="heroPortrait" src="assets/portraits/' +
        $Fighter.id +
        '.png" alt="' +
        (ConvertTo-HtmlText $Fighter.name) +
        '">'
    )
    $Html = $Html -replace (
        '<p id="fighterSubtitle" class="fighter-subtitle"></p>'
    ), (
        '<p id="fighterSubtitle" class="fighter-subtitle">' +
        $MoveMarkup.Count +
        " приёмов</p>"
    )
    $Html = [regex]::Replace(
        $Html,
        '(?s)<div id="rosterList" class="roster-list" role="listbox"></div>',
        "<div id=""rosterList"" class=""roster-list"" role=""listbox"">`r`n" +
            $RosterMarkup +
            "`r`n                </div>"
    )
    $Html = [regex]::Replace(
        $Html,
        '(?s)<div id="pageContent" class="page-content"></div>',
        "<div id=""pageContent"" class=""page-content"">`r`n" +
            $MoveMarkup.Html +
            "`r`n                        </div>"
    )
    if (-not $PlatformInfo.Available) {
        $Notice = if ($PlatformInfo.Note) {
            $PlatformInfo.Note
        } else {
            "$($Fighter.name) недоступен в выбранной версии игры."
        }
        $Html = [regex]::Replace(
            $Html,
            '(?s)<div id="platformNotice" class="platform-notice"\s+role="note" hidden></div>',
            '<div id="platformNotice" class="platform-notice" role="note">' +
                "<strong>НЕТ В SEGA</strong><span>" +
                (ConvertTo-HtmlText $Notice) +
                "</span></div>"
        )
    }
    return $Html
}

if (Test-Path -LiteralPath $OutputPath) {
    Remove-Item -LiteralPath $OutputPath -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputPath | Out-Null
Get-ChildItem -LiteralPath $WebPath -Force | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination $OutputPath -Recurse -Force
}

$SitemapUrls = New-Object System.Collections.Generic.List[string]
function Write-SitePage {
    param(
        [string]$RelativeDirectory,
        [ValidateSet("root", "platform", "fighter")]
        [string]$Kind,
        [string]$Platform,
        [object]$Fighter
    )
    $RelativeUrl = if ($RelativeDirectory) {
        $RelativeDirectory.Trim("/") + "/"
    } else {
        ""
    }
    $CanonicalUrl = $NormalizedBaseUrl + $RelativeUrl
    $Directory = if ($RelativeDirectory) {
        Join-Path $OutputPath (
            $RelativeDirectory -replace "/", [IO.Path]::DirectorySeparatorChar
        )
    } else {
        $OutputPath
    }
    New-Item -ItemType Directory -Path $Directory -Force | Out-Null
    $Html = New-StaticPage `
        -Kind $Kind `
        -Platform $Platform `
        -Fighter $Fighter `
        -CanonicalUrl $CanonicalUrl
    [IO.File]::WriteAllText(
        (Join-Path $Directory "index.html"),
        $Html,
        $Utf8NoBom
    )
    $SitemapUrls.Add($CanonicalUrl)
}

$DefaultFighter = $Fighters[0]
Write-SitePage `
    -RelativeDirectory "" `
    -Kind "root" `
    -Platform "arcade" `
    -Fighter $DefaultFighter

foreach ($Platform in @("arcade", "sega")) {
    Write-SitePage `
        -RelativeDirectory $Platform `
        -Kind "platform" `
        -Platform $Platform `
        -Fighter $DefaultFighter
    foreach ($Fighter in $Fighters) {
        Write-SitePage `
            -RelativeDirectory "$Platform/$($Fighter.id)" `
            -Kind "fighter" `
            -Platform $Platform `
            -Fighter $Fighter
    }
}

$SitemapEntries = foreach ($Url in $SitemapUrls) {
@"
  <url>
    <loc>$(ConvertTo-HtmlText $Url)</loc>
  </url>
"@
}
$Sitemap = @"
<?xml version="1.0" encoding="UTF-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
$($SitemapEntries -join [Environment]::NewLine)
</urlset>
"@
$Robots = @"
User-agent: *
Allow: /

Sitemap: $($NormalizedBaseUrl)sitemap.xml
"@

[IO.File]::WriteAllText(
    (Join-Path $OutputPath "sitemap.xml"),
    $Sitemap,
    $Utf8NoBom
)
[IO.File]::WriteAllText(
    (Join-Path $OutputPath "robots.txt"),
    $Robots,
    $Utf8NoBom
)
[IO.File]::WriteAllText(
    (Join-Path $OutputPath ".nojekyll"),
    "",
    $Utf8NoBom
)

Write-Host (
    "Generated {0} pages in {1}" -f $SitemapUrls.Count, $OutputPath
)
