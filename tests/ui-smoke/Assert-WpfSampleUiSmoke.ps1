param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",

    [ValidateRange(1, 120)]
    [int]$TimeoutSeconds = 15
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryRoot = Resolve-Path (Join-Path $scriptRoot "..\..")
$exePath = Join-Path $repositoryRoot "ATS5.Wpf\bin\$Configuration\net461\ATS5.Wpf.exe"
$expectedTitle = -join @([char]0x6D4B, [char]0x8BD5, [char]0x7CFB, [char]0x7EDF)
$process = $null

function Write-Pass {
    param([string]$Message)
    Write-Host "PASS: $Message"
}

function Write-Fail {
    param([string]$Message)
    Write-Host "FAIL: $Message"
}

try {
    if (-not (Test-Path -LiteralPath $exePath)) {
        throw "WPF executable not found: $exePath. Run dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore first."
    }

    $process = Start-Process -FilePath $exePath -WorkingDirectory (Split-Path -Parent $exePath) -PassThru -WindowStyle Minimized
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    $mainWindowHandle = [IntPtr]::Zero
    $mainWindowTitle = ""

    while ((Get-Date) -lt $deadline) {
        if ($process.HasExited) {
            throw "WPF process exited before creating a main window. ExitCode=$($process.ExitCode)"
        }

        $process.Refresh()
        if ($process.MainWindowHandle -ne [IntPtr]::Zero) {
            $mainWindowHandle = $process.MainWindowHandle
            $mainWindowTitle = $process.MainWindowTitle
            break
        }

        Start-Sleep -Milliseconds 250
    }

    if ($mainWindowHandle -eq [IntPtr]::Zero) {
        throw "Timed out after $TimeoutSeconds seconds waiting for the WPF main window handle."
    }

    if ([string]::IsNullOrWhiteSpace($mainWindowTitle)) {
        throw "WPF main window handle was created, but the window title is empty."
    }

    if ($mainWindowTitle -notlike "*$expectedTitle*") {
        throw "Unexpected WPF main window title: '$mainWindowTitle'."
    }

    Write-Pass "Launched $exePath"
    Write-Pass "MainWindowHandle=$mainWindowHandle"
    Write-Pass "MainWindowTitle=$mainWindowTitle"
    Write-Pass "No business workflow was clicked or executed."
}
catch {
    Write-Fail $_.Exception.Message
    exit 1
}
finally {
    if ($process -ne $null -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
        Write-Host "INFO: Stopped process Id=$($process.Id)"
    }
}
