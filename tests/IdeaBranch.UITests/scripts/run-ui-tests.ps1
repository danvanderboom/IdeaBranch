# PowerShell script to run UI tests with WinAppDriver
# Usage: .\run-ui-tests.ps1 [-Configuration Debug|Release] [-AppPath <path>]

param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',
    
    [Parameter()]
    [string]$AppPath = '',
    
    [Parameter()]
    [string]$WinAppDriverPath = 'C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe',
    
    [Parameter()]
    [string]$TestFilter = '',
    
    [Parameter()]
    [switch]$SkipWinAppDriver,
    
    [Parameter()]
    [switch]$Cleanup
)

$ErrorActionPreference = 'Stop'

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Running Idea Branch UI Tests" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# Get script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$testProjectDir = Split-Path -Parent $scriptDir
$rootDir = Split-Path -Parent (Split-Path -Parent $testProjectDir)

Write-Host "`nConfiguration: $Configuration" -ForegroundColor Yellow
Write-Host "Root Directory: $rootDir" -ForegroundColor Yellow

# Resolve app path
if ([string]::IsNullOrWhiteSpace($AppPath)) {
    $appExePath = Join-Path $rootDir "src\IdeaBranch.App\bin\$Configuration\net9.0-windows10.0.19041.0\win10-x64\IdeaBranch.App.exe"
} else {
    $appExePath = $AppPath
}

Write-Host "App Path: $appExePath" -ForegroundColor Yellow

# Verify app exists
if (-not (Test-Path $appExePath)) {
    Write-Error "App executable not found: $appExePath`nPlease build the app first."
    exit 1
}

# Set environment variable
$env:MAUI_APP_PATH = $appExePath
if (Test-Path $WinAppDriverPath) {
    $env:WINAPPDRIVER_PATH = $WinAppDriverPath
}

# Start WinAppDriver
$winAppDriverProcess = $null
if (-not $SkipWinAppDriver) {
    Write-Host "`nStarting WinAppDriver..." -ForegroundColor Green
    
    if (-not (Test-Path $WinAppDriverPath)) {
        Write-Warning "WinAppDriver not found at: $WinAppDriverPath"
        Write-Warning "Assuming WinAppDriver is already running or will be started manually"
    } else {
        try {
            # Check if WinAppDriver is already running
            $existingProcess = Get-Process -Name "WinAppDriver" -ErrorAction SilentlyContinue
            if ($existingProcess) {
                Write-Host "WinAppDriver is already running (PID: $($existingProcess.Id))" -ForegroundColor Yellow
            } else {
                # Start WinAppDriver
                $winAppDriverProcess = Start-Process -FilePath $WinAppDriverPath -PassThru -WindowStyle Hidden
                Write-Host "WinAppDriver started (PID: $($winAppDriverProcess.Id))" -ForegroundColor Green
                
                # Wait for WinAppDriver to be ready
                Write-Host "Waiting for WinAppDriver to be ready..." -ForegroundColor Yellow
                $maxRetries = 10
                $retryCount = 0
                $ready = $false
                
                while ($retryCount -lt $maxRetries -and -not $ready) {
                    Start-Sleep -Seconds 1
                    try {
                        $response = Invoke-WebRequest -Uri "http://127.0.0.1:4723/status" -TimeoutSec 2 -ErrorAction Stop
                        if ($response.StatusCode -eq 200) {
                            $ready = $true
                            Write-Host "WinAppDriver is ready!" -ForegroundColor Green
                        }
                    } catch {
                        $retryCount++
                        if ($retryCount -ge $maxRetries) {
                            Write-Warning "WinAppDriver may not be ready yet, but continuing..."
                        }
                    }
                }
            }
        } catch {
            Write-Warning "Could not start WinAppDriver: $_"
            Write-Warning "Assuming WinAppDriver is already running or will be started manually"
        }
    }
}

# Build test command
$testProjectPath = Join-Path $testProjectDir "IdeaBranch.UITests.csproj"
$testArgs = @(
    "test",
    "`"$testProjectPath`"",
    "-c", $Configuration,
    "--logger", "trx;LogFileName=test-results.trx",
    "--logger", "console;verbosity=normal"
)

if (-not [string]::IsNullOrWhiteSpace($TestFilter)) {
    $testArgs += "--filter"
    $testArgs += "`"$TestFilter`""
}

Write-Host "`nRunning UI tests..." -ForegroundColor Green
Write-Host "Command: dotnet $($testArgs -join ' ')" -ForegroundColor Gray

# Run tests
$testExitCode = 0
try {
    & dotnet $testArgs
    $testExitCode = $LASTEXITCODE
} catch {
    Write-Error "Error running tests: $_"
    $testExitCode = 1
}

# Cleanup WinAppDriver if we started it
if ($winAppDriverProcess -and $Cleanup) {
    Write-Host "`nStopping WinAppDriver..." -ForegroundColor Yellow
    try {
        Stop-Process -Id $winAppDriverProcess.Id -Force -ErrorAction SilentlyContinue
        Write-Host "WinAppDriver stopped" -ForegroundColor Green
    } catch {
        Write-Warning "Could not stop WinAppDriver: $_"
    }
}

# Report results
Write-Host "`n=========================================" -ForegroundColor Cyan
if ($testExitCode -eq 0) {
    Write-Host "Tests completed successfully!" -ForegroundColor Green
} else {
    Write-Host "Tests failed with exit code: $testExitCode" -ForegroundColor Red
}
Write-Host "=========================================" -ForegroundColor Cyan

# Check for test results
$testResultsPath = Join-Path $testProjectDir "TestResults\test-results.trx"
if (Test-Path $testResultsPath) {
    Write-Host "Test results: $testResultsPath" -ForegroundColor Yellow
}

exit $testExitCode

