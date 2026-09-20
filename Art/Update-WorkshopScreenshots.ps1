<#
.SYNOPSIS
    Collects the screenshots the Pickle @review scenarios take into Art/Workshop/, named for the
    Steam page rather than for the test that produced them.

.DESCRIPTION
    Tests/Pickle's 14-publication-shots.feature builds a scene worth showing - a work type named the
    way a player would name it, real tasks moved into it - and photographs it with the interface
    around the windows hidden, through the game's own screenshot mode. Run that feature with the
    game IN ENGLISH: the Workshop page is English.

    Older note, kept because it is still true of the  captures:

        Tests/Pickle's @review features (03-three-columns, 07b-rename-visual, 10b-drift-warning-wording,
    13-settings-window) exist so a person can judge what no assertion can. They also happen to
    photograph exactly what the Workshop page needs to show, in a clean fixture colony, at a
    consistent size, every run. This copies that set out of the report folder under stable names.

    Nothing is deleted and nothing is uploaded: the files land in Art/Workshop/ and go to Steam by
    hand, like Preview.png does. Art/ is not shipped with the mod, so none of this reaches
    subscribers.

    These images are large (~2 MB each) and change on every run - different time of day, colonists
    standing elsewhere. Commit a set when it is one you would publish, not automatically, or the
    history fills with near-identical captures.

.PARAMETER ReportFolder
    Where Pickle writes its screenshots. Defaults to the RimWorld save-data folder of the current
    user.

.PARAMETER Force
    Copy even when the destination already holds an identical file.

.EXAMPLE
    .\Art\Update-WorkshopScreenshots.ps1
#>
[CmdletBinding()]
param(
    [string] $ReportFolder = (Join-Path $env:USERPROFILE 'AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\PickleReports\screenshots'),
    [switch] $Force
)

$ErrorActionPreference = 'Stop'

$wanted = [ordered] @{
    # Pickle names a manual screenshot after the label the step passed, turning every character
    # that is not a letter or a digit into a dash - so "Workshop page, the editor" becomes
    # "Workshop-page--the-editor". A renamed screenshot step must be renamed here too; the script
    # says which one it could not find rather than silently producing a short set.
    "manual--Workshop-page--the-editor--step0.png"      = "01-the-editor.png"
    "manual--Workshop-page--the-new-column--step0.png"  = "02-the-new-column.png"
    "manual--Workshop-page--the-settings--step0.png"    = "03-the-settings.png"
}

if (-not (Test-Path -LiteralPath $ReportFolder)) {
    throw "No Pickle screenshot folder at '$ReportFolder'. Run the suite first, or pass -ReportFolder."
}

$destination = Join-Path $PSScriptRoot 'Workshop'
if (-not (Test-Path -LiteralPath $destination)) {
    New-Item -ItemType Directory -Path $destination | Out-Null
}

$copied = 0
$skipped = 0
$missing = @()

foreach ($source in $wanted.Keys) {
    $from = Join-Path $ReportFolder $source
    $to = Join-Path $destination $wanted[$source]

    if (-not (Test-Path -LiteralPath $from)) {
        $missing += $source
        continue
    }

    $identical = $false
    if (-not $Force -and (Test-Path -LiteralPath $to)) {
        $fromHash = (Get-FileHash -LiteralPath $from -Algorithm SHA256).Hash
        $toHash = (Get-FileHash -LiteralPath $to -Algorithm SHA256).Hash
        $identical = $fromHash -eq $toHash
    }

    if ($identical) {
        $skipped++
        Write-Host "  unchanged  $($wanted[$source])"
        continue
    }

    Copy-Item -LiteralPath $from -Destination $to -Force
    $copied++
    $taken = (Get-Item -LiteralPath $from).LastWriteTime
    Write-Host "  copied     $($wanted[$source])   (taken $taken)"
}

Write-Host ''
Write-Host "$copied copied, $skipped already current, into $destination"

if ($missing.Count -gt 0) {
    Write-Warning ("Not in the report folder: " + ($missing -join ', '))
    Write-Warning 'Either the suite has not run since those scenarios were written, or a screenshot step was renamed and this script needs the new name.'
}

Write-Host 'Review them before committing: they change on every run, and only a set worth publishing belongs in the history.'
