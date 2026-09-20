<#
    Runs this mod's Pickle suite, under a lock no second session can take.

    One machine has one RimWorld, and Pickle has one runner inside it. Two sessions starting a run
    at the same time is the failure this exists for: the second launch dies on Steam's single
    instance, and a second /run lands on a runner that is already busy. Both leave the report
    unwritten, which is how an afternoon gets spent reading a report from hours earlier.

    This script cannot start RimWorld, and that is deliberate. It drives the game that is already
    open, through Pickle's dashboard, and does nothing at all when none is. It used to have a
    -Launch switch guarded by a Get-Process check, and on 2026-09-20 that guard let a launch
    through against a game that was still coming up: a game starting is not yet a process, so no
    check of this kind can see it. A second RimWorld cuts off whatever the first was doing - the
    same evening, one killed SkillIcons' run, which died without writing a report and left nothing
    in the next one to say why. The switch is gone rather than hardened, because the rule is not to
    launch, not to launch carefully.

    Starting the game is the owner's, by hand:

        RimWorldWin64.exe "-pickle-run=Work Studio - Pickle tests"

    which runs the suite unattended and closes its own game at the end. A game that was already
    open is never closed by anything here: it carries work nothing shows, and that is its owner's
    call.

    The assemblies are read at startup, so a game older than the build tests the previous one while
    you read the new one's source. The script refuses that rather than let it pass unnoticed - it
    is the one case where the answer is to restart the game, which is again yours to do.

    PickleReports holds one report for the whole machine, not one per mod: summary.md, junit.xml
    and the rest are rewritten by whichever suite ran last. On 2026-09-20 this suite was believed
    to have run and the report read that evening was Architect Studio's, taken at the same minute.
    That is why the outcome below is read back from the runner's own per-mod state rather than
    from the report folder: /state names the mod each scenario belongs to, and a scenario this
    game has never run reads Pending, which no report can tell you.

    For the same reason the previous report is moved to PickleReports-archive before a run starts,
    under the hour it was written, and the oldest are dropped past -KeepReports. An archive is a
    reprieve, not storage: a session that needs a report copies what it needs somewhere of its own.
#>
[CmdletBinding()]
param(
    [string]$Mod = 'Work Studio - Pickle tests',
    [int]$Port = 27750,
    [int]$TimeoutMinutes = 90,
    [int]$KeepReports = 5,
    [switch]$Force
)


# -Launch is refused since 2026-09-21. Starting the Windows game takes her screen, and on
# 2026-09-20 three sessions did it without her asking. A suite runs in the WSL install now:
#   pwsh -File scripts/Run-PickleWsl.ps1 -Mod <Mod>
# What remains here is driving a game SHE already has open, through Pickle's dashboard.
if ($Launch) {
    Write-Host 'Lancer le RimWorld de Windows est interdit : il prend son ecran.' -ForegroundColor Red
    Write-Host 'Utilise  pwsh -File scripts/Run-PickleWsl.ps1 -Mod <Mod>  (WSL, sous Xvfb).' -ForegroundColor Yellow
    exit 5
}

$ErrorActionPreference = 'Stop'

$lockPath = Join-Path $env:LOCALAPPDATA 'rimworld-pickle-run.lock'
$gamePath = 'C:\Program Files (x86)\Steam\steamapps\common\RimWorld\RimWorldWin64.exe'
$saveData = Join-Path $env:USERPROFILE 'AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios'
$reportRoot = Join-Path $saveData 'PickleReports'
$archiveRoot = "$reportRoot-archive"
$configRoot = Join-Path $saveData 'Config'
$origin = "http://localhost:$Port"

# Everything the game reads once, at startup.
$assemblies = @(
    (Join-Path $PSScriptRoot 'Mod\Pickle\Assemblies\WorkStudio.PickleSteps.dll'),
    (Join-Path $PSScriptRoot '..\..\Mod\Assemblies\WorkStudio.dll')
)

function Get-Game {
    return Get-Process -Name RimWorldWin64 -ErrorAction SilentlyContinue
}

# The step assembly and the mod assembly are read when the game starts. Driving a game older than
# the build means testing the previous build while reading the new one's source, which is a whole
# evening if it goes unnoticed - so it is checked here rather than left to whoever remembers.
function Assert-BuildOlderThanGame($game) {
    $newer = @()
    foreach ($path in $assemblies) {
        if (-not (Test-Path -LiteralPath $path)) { continue }
        $built = (Get-Item -LiteralPath $path).LastWriteTime
        if ($built -gt $game.StartTime) {
            $newer += "  $(Split-Path -Leaf $path) built $($built.ToString('HH:mm:ss')), game started $($game.StartTime.ToString('HH:mm:ss'))"
        }
    }
    if ($newer.Count -gt 0) {
        throw ("This game started before the build it would be running:`n" + ($newer -join "`n") +
               "`nRestart the game yourself to load it: this script never starts or closes one.")
    }
}

# A scenario that dies leaves its restores undone. The settings backup is the one that matters to
# the next session: Config/ is shared, and a backup left there is the mod's real settings waiting
# to be put back by hand.
function Show-Leftovers($when) {
    if (-not (Test-Path -LiteralPath $configRoot)) { return }
    $backups = @(Get-ChildItem -LiteralPath $configRoot -Filter '*.pickle-backup' -ErrorAction SilentlyContinue)
    if ($backups.Count -eq 0) { return }
    Write-Warning "$when : $($backups.Count) .pickle-backup left in Config/ - a scenario died without restoring the settings it saved."
    foreach ($backup in $backups) { Write-Warning "  $($backup.Name)   (written $($backup.LastWriteTime.ToString('HH:mm:ss')))" }
    Write-Warning '  Copy it back over the file it shadows before trusting any settings reading, here or in the next mod audited.'
}

function Get-RunnerState {
    $response = Invoke-WebRequest -UseBasicParsing -Uri "$origin/state" -TimeoutSec 10
    return $response.Content | ConvertFrom-Json
}

function Invoke-Runner($path) {
    Invoke-RestMethod -Method Post -Uri "$origin$path" -Headers @{ Origin = $origin } -TimeoutSec 30 | Out-Null
}

# CreateNew is the whole point: the file system decides who gets the lock, not a Test-Path that
# another session can win between the test and the write.
function Enter-Lock {
    for ($attempt = 0; $attempt -lt 2; $attempt++) {
        try {
            $stream = [System.IO.File]::Open($lockPath, 'CreateNew', 'Write', 'None')
            $holder = @{ pid = $PID; host = $env:COMPUTERNAME; taken = (Get-Date).ToString('o'); mod = $Mod } | ConvertTo-Json -Compress
            $bytes = [System.Text.Encoding]::UTF8.GetBytes($holder)
            $stream.Write($bytes, 0, $bytes.Length)
            $stream.Close()
            return
        } catch [System.IO.IOException] {
            $held = $null
            try { $held = Get-Content $lockPath -Raw -ErrorAction Stop | ConvertFrom-Json } catch { }
            $alive = $false
            if ($held) { $alive = $null -ne (Get-Process -Id $held.pid -ErrorAction SilentlyContinue) }
            if ($alive -and -not $Force) {
                throw "A Pickle run is already held by process $($held.pid), taken at $($held.taken) for '$($held.mod)'. Wait for it, or pass -Force once you know that run is over."
            }
            # -Force exists for a holder that is alive but no longer running anything: a shell left
            # open behind a finished run. It is not a way past a run in progress, so the runner is
            # asked before the lock is taken from someone who still answers.
            if ($alive) {
                $busy = $null
                try { $busy = (Get-RunnerState).status } catch { }
                if ($busy -and $busy -ne 'idle') {
                    throw "Process $($held.pid) holds the lock for '$($held.mod)' and the runner is $busy. -Force does not take a run away from a run in progress: wait for it."
                }
                Write-Host "Taking the lock from process $($held.pid), which still lives but is not running anything (taken at $($held.taken))."
            }
            if ($held -and -not $alive) { Write-Host "Taking the lock from process $($held.pid), which is gone (taken at $($held.taken))." }
            Remove-Item $lockPath -Force
        }
    }
    throw "Could not take $lockPath."
}

function Exit-Lock {
    Remove-Item $lockPath -Force -ErrorAction SilentlyContinue
}

# Pickle writes every run into one folder and overwrites what was there, screenshots included. The
# run that is about to start would therefore destroy the evidence of the one before it - which is
# how a failure spends an afternoon being read from a report that belonged to another run. Moving
# the old one out costs nothing on the same volume, and leaves the live folder empty rather than
# growing without end.
#
# This keeps the last $KeepReports runs by count, not by age: a day with six runs would otherwise
# lose the one that mattered, and a quiet week would keep nothing but stale ones.
#
# An archive is not storage. A session that needs a report must copy what it needs somewhere of
# its own, and clean up after itself: the runs that follow will push this one out.
#
# Taken from Architect Studio's copy of this script, 2026-09-20.
function Save-PreviousReport {
    $marker = Join-Path $reportRoot 'junit.xml'
    if (-not (Test-Path $marker)) { return }

    $stamp = (Get-Item $marker).LastWriteTime.ToString('yyyy-MM-dd_HHmm')
    $target = Join-Path $archiveRoot $stamp
    $twin = 1
    while (Test-Path $target) {
        $twin++
        $target = Join-Path $archiveRoot "$stamp-$twin"
    }
    New-Item -ItemType Directory -Path $target -Force | Out-Null
    Get-ChildItem -LiteralPath $reportRoot | Move-Item -Destination $target

    # The report names its screenshots by absolute path, into the folder the next run is about to
    # fill: left as they are, an archived report would show the wrong images, which is worse than
    # showing none.
    # Pickle writes those paths with mixed separators - forward slashes down to the save folder,
    # backslashes after it - so they are matched separator by separator rather than literally.
    $anySeparator = ($reportRoot -split '[\\/]' | ForEach-Object { [regex]::Escape($_) }) -join '[\\/]'
    $utf8 = New-Object System.Text.UTF8Encoding $false
    foreach ($file in (Get-ChildItem -LiteralPath $target -File | Where-Object { $_.Extension -in '.xml', '.html', '.md', '.json', '.ndjson' })) {
        $text = [System.IO.File]::ReadAllText($file.FullName)
        $rewritten = [regex]::Replace($text, $anySeparator, '.', 'IgnoreCase')
        if ($rewritten -ne $text) { [System.IO.File]::WriteAllText($file.FullName, $rewritten, $utf8) }
    }
    Write-Host "Previous report kept in $target."

    $stale = @(Get-ChildItem -LiteralPath $archiveRoot -Directory | Sort-Object LastWriteTime -Descending | Select-Object -Skip $KeepReports)
    foreach ($directory in $stale) {
        Remove-Item -LiteralPath $directory.FullName -Recurse -Force
        Write-Host "Dropped $($directory.Name): older than the last $KeepReports runs."
    }
}

function Show-Outcome($startedAt) {
    $state = Get-RunnerState
    $scenarios = @($state.features | Where-Object { $_.mod -eq $Mod } | ForEach-Object { $_.scenarios })
    $passed = @($scenarios | Where-Object { $_.outcome -eq 'Passed' })
    $failed = @($scenarios | Where-Object { $_.outcome -eq 'Failed' })
    $skipped = @($scenarios | Where-Object { $_.outcome -eq 'Skipped' }).Count
    $pending = @($scenarios | Where-Object { $_.outcome -eq 'Pending' }).Count

    Write-Host ""
    Write-Host "$Mod : $($passed.Count) passed, $($failed.Count) failed, $skipped skipped"
    if ($pending -gt 0) {
        Write-Warning "$pending scenarios of this mod are still Pending: the run did not reach them."
    }
    foreach ($scenario in $failed) {
        Write-Host ""
        Write-Host "FAILED: $($scenario.name)"
        Write-Host "  $($scenario.failureMessage)"
        foreach ($attachment in $scenario.attachments) {
            if ($attachment.name -eq 'smoke-attachment') { continue }
            Write-Host "  [$($attachment.name)] $($attachment.content)"
        }
    }

    # A @review scenario asserts nothing. Green means the trip happened, not that the picture is
    # right, so it is named rather than left to swell a count that reads like verification.
    $review = @($passed | Where-Object { $_.tags -contains 'review' -or $_.tags -contains '@review' })
    if ($review.Count -gt 0) {
        Write-Host ""
        Write-Host "$($review.Count) of those passes assert nothing - they attach screenshots for a person to read:"
        foreach ($scenario in $review) { Write-Host "  $($scenario.name)" }
    }

    # PickleReports is machine-wide and belongs to whichever suite ended last. Saying whether the
    # files down there are this run's is the difference between a report and someone else's.
    Write-Host ""
    $junit = Join-Path $reportRoot 'junit.xml'
    if (Test-Path -LiteralPath $junit) {
        $written = (Get-Item -LiteralPath $junit).LastWriteTime
        if ($written -ge $startedAt) {
            Write-Host "Report (this run, written $($written.ToString('HH:mm:ss'))): $reportRoot"
        } else {
            Write-Warning "The report in $reportRoot was written at $($written.ToString('HH:mm:ss')), before this run started at $($startedAt.ToString('HH:mm:ss')). It is another suite's. The outcome above comes from the runner, not from it."
        }
    } else {
        Write-Warning "No junit.xml in $reportRoot. The outcome above comes from the runner."
    }

    Show-Leftovers 'After the run'
    Write-Host ''
    Write-Host "Left as this script found it, except: the runner's search box is cleared and its selection is now '$Mod'."
    Write-Host 'Anything a scenario changed in the game itself - language, interface scale, dev mode, the mod list - it restores itself; what it does not is the running game, which this script never closes.'
    if ($failed.Count -gt 0) { exit 1 }
}

Enter-Lock
try {
    Show-Leftovers 'Before the run'

    $game = Get-Game
    if (-not $game) {
        throw ("No RimWorld is running, and this script does not start one. Open the game yourself, then run this " +
               "again - or run the suite unattended with:`n" +
               "    & '$gamePath' '-pickle-run=`"$Mod`"'`n" +
               'which closes its own game at the end. The same command is what a build newer than the running game needs.')
    }
    Assert-BuildOlderThanGame $game

    $state = Get-RunnerState
    if ($state.status -ne 'idle') {
        throw "The runner is $($state.status) on '$($state.scenario)'. Someone else is driving this game."
    }

    # The search box is remembered between runs, and a scenario it hides is a scenario the
    # selection below silently misses.
    Invoke-Runner '/filter?search=&mod=&clearTags=true'
    Invoke-Runner '/select?scope=none'
    Invoke-Runner ('/select?on=true&mod=' + [uri]::EscapeDataString($Mod))

    $state = Get-RunnerState
    $selected = @($state.features | ForEach-Object { $_.scenarios } | Where-Object { $_.selected }).Count
    if ($selected -eq 0) { throw "Nothing matched '$Mod': check the mod name against the runner's own list." }
    Write-Host "Running $selected scenarios of '$Mod'. The pointer moves on its own: leave the mouse alone."
    Save-PreviousReport

    $startedAt = Get-Date
    Invoke-Runner '/scope?value=selected'
    Invoke-Runner '/run?scope=selected'

    $deadline = (Get-Date).AddMinutes($TimeoutMinutes)
    do {
        Start-Sleep -Seconds 15
        if ((Get-Date) -gt $deadline) { throw "The run is still going after $TimeoutMinutes minutes. It is left running; abort it from the dashboard at $origin/." }
        $state = Get-RunnerState
    } while ($state.status -eq 'running')

    Show-Outcome $startedAt
} finally {
    Exit-Lock
}
