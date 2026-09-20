<#
    Runs this mod's Pickle suite, under a lock no second session can take.

    One machine has one RimWorld, and Pickle has one runner inside it. Two sessions starting a run
    at the same time is the failure this exists for: the second launch dies on Steam's single
    instance, and a second /run lands on a runner that is already busy. Both leave the report
    unwritten, which is how an afternoon gets spent reading a report from hours earlier.

    Without -Launch the script drives the game that is already open, through Pickle's dashboard:
    the step assembly is read at startup, so this only tests a build older than that game. With
    -Launch it starts one, which is what a freshly built step assembly needs.

    The game is never closed by this script. An unattended run closes it by itself when it ends;
    a game that was already open stays open.

    PickleReports holds one report for the whole machine, not one per mod: summary.md, junit.xml
    and the rest are rewritten by whichever suite ran last. On 2026-09-20 this suite was believed
    to have run and the report read that evening was Architect Studio's, taken at the same minute.
    That is why the outcome below is read back from the runner's own per-mod state rather than
    from the report folder: /state names the mod each scenario belongs to, and a scenario this
    game has never run reads Pending, which no report can tell you.
#>
[CmdletBinding()]
param(
    [string]$Mod = 'Work Studio - Pickle tests',
    [int]$Port = 27750,
    [switch]$Launch,
    [int]$TimeoutMinutes = 90,
    [switch]$Force
)

$ErrorActionPreference = 'Stop'

$lockPath = Join-Path $env:LOCALAPPDATA 'rimworld-pickle-run.lock'
$gamePath = 'C:\Program Files (x86)\Steam\steamapps\common\RimWorld\RimWorldWin64.exe'
$reportRoot = Join-Path $env:USERPROFILE 'AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\PickleReports'
$origin = "http://localhost:$Port"

function Test-GameRunning {
    return $null -ne (Get-Process -Name RimWorldWin64 -ErrorAction SilentlyContinue)
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
            if ($held) { Write-Host "Taking the lock from process $($held.pid), which is gone (taken at $($held.taken))." }
            Remove-Item $lockPath -Force
        }
    }
    throw "Could not take $lockPath."
}

function Exit-Lock {
    Remove-Item $lockPath -Force -ErrorAction SilentlyContinue
}

function Show-Outcome {
    $state = Get-RunnerState
    $scenarios = @($state.features | Where-Object { $_.mod -eq $Mod } | ForEach-Object { $_.scenarios })
    $passed = @($scenarios | Where-Object { $_.outcome -eq 'Passed' }).Count
    $failed = @($scenarios | Where-Object { $_.outcome -eq 'Failed' })
    $skipped = @($scenarios | Where-Object { $_.outcome -eq 'Skipped' }).Count

    Write-Host ""
    Write-Host "$Mod : $passed passed, $($failed.Count) failed, $skipped skipped"
    foreach ($scenario in $failed) {
        Write-Host ""
        Write-Host "FAILED: $($scenario.name)"
        Write-Host "  $($scenario.failureMessage)"
        foreach ($attachment in $scenario.attachments) {
            if ($attachment.name -eq 'smoke-attachment') { continue }
            Write-Host "  [$($attachment.name)] $($attachment.content)"
        }
    }
    Write-Host ""
    Write-Host "Report: $reportRoot"
    if ($failed.Count -gt 0) { exit 1 }
}

Enter-Lock
try {
    if ($Launch) {
        if (Test-GameRunning) {
            throw 'RimWorld is already running. Drive it without -Launch, or let whoever is playing finish: this script never closes the game.'
        }
        # The filter is the companion mod's name, exactly; PowerShell would otherwise cut the
        # argument at the first space and Pickle would find no scenario at all.
        $arguments = "-pickle-run=`"$Mod`""
        Write-Host "Launching RimWorld for '$Mod'..."
        $game = Start-Process -FilePath $gamePath -ArgumentList $arguments -PassThru
        $deadline = (Get-Date).AddMinutes($TimeoutMinutes)
        while (-not $game.HasExited) {
            if ((Get-Date) -gt $deadline) { throw "The run is still going after $TimeoutMinutes minutes; the game is left alone, check it yourself." }
            Start-Sleep -Seconds 15
        }
        $junit = Join-Path $reportRoot 'junit.xml'
        if (-not (Test-Path $junit)) { throw "The game exited without writing ${junit}: the run never reached its end." }
        Write-Host "Report written: $junit"
        return
    }

    if (-not (Test-GameRunning)) {
        throw 'No RimWorld is running. Start one with -Launch, which is also what a step assembly built since that game started needs.'
    }

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

    Invoke-Runner '/scope?value=selected'
    Invoke-Runner '/run?scope=selected'

    $deadline = (Get-Date).AddMinutes($TimeoutMinutes)
    do {
        Start-Sleep -Seconds 15
        if ((Get-Date) -gt $deadline) { throw "The run is still going after $TimeoutMinutes minutes. It is left running; abort it from the dashboard at $origin/." }
        $state = Get-RunnerState
    } while ($state.status -eq 'running')

    Show-Outcome
} finally {
    Exit-Lock
}
