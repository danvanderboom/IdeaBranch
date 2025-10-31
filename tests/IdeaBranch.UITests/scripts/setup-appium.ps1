# PowerShell script to set up Appium/WinAppDriver for UI tests
# Usage: .\setup-appium.ps1 [-Install] [-Start] [-Verify]

param(
    [Parameter()]
    [switch]$Install,
    
    [Parameter()]
    [switch]$Start,
    
    [Parameter()]
    [switch]$Verify,
    
    [Parameter()]
    [string]$WinAppDriverPath = 'C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe',
    
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Appium/WinAppDriver Setup Script" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# Function to check if running as administrator
function Test-Administrator {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

# Get script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$testProjectDir = Split-Path -Parent $scriptDir
$rootDir = Split-Path -Parent (Split-Path -Parent $testProjectDir)

# Function to check if WinAppDriver is installed
function Test-WinAppDriverInstalled {
    param([string]$Path)
    
    if (Test-Path $Path) {
        Write-Host "`nOK: WinAppDriver found at: $Path" -ForegroundColor Green
        return $true
    } else {
        Write-Host "`nFAILED: WinAppDriver not found at: $Path" -ForegroundColor Red
        return $false
    }
}

# Function to check if Developer Mode is enabled
function Test-DeveloperMode {
    $developerMode = Get-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock" -Name "AllowDevelopmentWithoutDevLicense" -ErrorAction SilentlyContinue
    
    if ($developerMode -and $developerMode.AllowDevelopmentWithoutDevLicense -eq 1) {
        Write-Host "OK: Developer Mode is enabled" -ForegroundColor Green
        return $true
    } else {
        Write-Host "FAILED: Developer Mode is NOT enabled" -ForegroundColor Red
        Write-Host "  To enable:" -ForegroundColor Yellow
        Write-Host "  1. Open Windows Settings (Win + I)" -ForegroundColor Yellow
        Write-Host "  2. Go to Privacy & Security → For developers" -ForegroundColor Yellow
        Write-Host "  3. Enable 'Developer Mode'" -ForegroundColor Yellow
        Write-Host "  4. Restart if prompted" -ForegroundColor Yellow
        return $false
    }
}

# Function to check if WinAppDriver is running
function Test-WinAppDriverRunning {
    $result = $false
    try {
        $response = Invoke-WebRequest -Uri "http://127.0.0.1:4723/status" -TimeoutSec 2 -ErrorAction Stop
        if ($response.StatusCode -eq 200) {
            Write-Host "WinAppDriver is running" -ForegroundColor Green
            $status = $response.Content | ConvertFrom-Json
            Write-Host "  Version: $($status.value.build.version)" -ForegroundColor Gray
            $result = $true
        }
    }
    catch {
        Write-Host "WinAppDriver is NOT running" -ForegroundColor Red
    }
    return $result
}

# Function to check if app is built
function Test-AppBuilt {
    param([string]$RootDir, [string]$Config)
    
    $appPath = Join-Path $RootDir "src\IdeaBranch.App\bin\$Config\net9.0-windows10.0.19041.0\win10-x64\IdeaBranch.App.exe"
    
    if (Test-Path $appPath) {
        Write-Host "OK: App executable found: $appPath" -ForegroundColor Green
        return $true, $appPath
    } else {
        Write-Host "FAILED: App executable NOT found: $appPath" -ForegroundColor Red
        Write-Host "  Building app..." -ForegroundColor Yellow
        return $false, $appPath
    }
}

# Function to install WinAppDriver
function Install-WinAppDriver {
    param([string]$Path)
    
    Write-Host "`nInstalling WinAppDriver..." -ForegroundColor Yellow
    
    # Check if already installed
    if (Test-Path $Path) {
        Write-Host "WinAppDriver is already installed at: $Path" -ForegroundColor Yellow
        return $true
    }
    
    # Check for administrator privileges
    if (-not (Test-Administrator)) {
        Write-Warning "Administrator privileges are required to install WinAppDriver" -ForegroundColor Yellow
        Write-Host "Please run PowerShell as Administrator and try again, or install manually:" -ForegroundColor Yellow
        Write-Host "1. Right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Cyan
        Write-Host "2. Navigate to this directory and run the script again with -Install" -ForegroundColor Cyan
        Write-Host "`nOr install manually:" -ForegroundColor Yellow
        Write-Host "1. Visit: https://github.com/microsoft/WinAppDriver/releases" -ForegroundColor Cyan
        Write-Host "2. Download the latest WindowsApplicationDriver-*.msi" -ForegroundColor Cyan
        Write-Host "3. Right-click the MSI file and select 'Install'" -ForegroundColor Cyan
        return $false
    }
    
    # Get latest WinAppDriver download URL
    Write-Host "Fetching latest WinAppDriver release..." -ForegroundColor Yellow
    try {
        $releasesUrl = "https://api.github.com/repos/microsoft/WinAppDriver/releases/latest"
        $release = Invoke-RestMethod -Uri $releasesUrl -ErrorAction Stop
        $msiAsset = $release.assets | Where-Object { $_.name -like "*.msi" } | Select-Object -First 1
        
        if (-not $msiAsset) {
            Write-Error "Could not find MSI installer in latest release"
            return $false
        }
        
        Write-Host "Downloading: $($msiAsset.name)" -ForegroundColor Yellow
        $downloadPath = Join-Path $env:TEMP $msiAsset.name
        Invoke-WebRequest -Uri $msiAsset.browser_download_url -OutFile $downloadPath -ErrorAction Stop
        
        Write-Host "Installing WinAppDriver (this may take a moment)..." -ForegroundColor Yellow
        $installArgs = @(
            "/i",
            "`"$downloadPath`"",
            "/quiet",
            "/norestart"
        )
        
        # Run installer (we should already be admin, but msiexec will prompt if needed)
        $process = Start-Process -FilePath "msiexec.exe" -ArgumentList $installArgs -Wait -PassThru -NoNewWindow
        
        if ($process.ExitCode -eq 0 -or $process.ExitCode -eq 3010) {
            Write-Host "OK: WinAppDriver installed successfully" -ForegroundColor Green
            
            # Clean up installer
            Remove-Item $downloadPath -ErrorAction SilentlyContinue
            
            return $true
        } else {
            Write-Error "WinAppDriver installation failed with exit code: $($process.ExitCode)"
            Write-Host "Common causes:" -ForegroundColor Yellow
            Write-Host "  - Installation requires administrator privileges" -ForegroundColor Yellow
            Write-Host "  - Conflicting installation or locked files" -ForegroundColor Yellow
            Write-Host "  - Antivirus software blocking the installation" -ForegroundColor Yellow
            Write-Host "`nTry installing manually:" -ForegroundColor Yellow
            Write-Host "1. Visit: https://github.com/microsoft/WinAppDriver/releases" -ForegroundColor Cyan
            Write-Host "2. Download: $($msiAsset.name)" -ForegroundColor Cyan
            Write-Host "3. Right-click the MSI file and select 'Install' (Run as Administrator)" -ForegroundColor Cyan
            return $false
        }
    } catch {
        Write-Error "Failed to install WinAppDriver: $_"
        Write-Host "`nManual installation:" -ForegroundColor Yellow
        Write-Host "1. Visit: https://github.com/microsoft/WinAppDriver/releases" -ForegroundColor Cyan
        Write-Host "2. Download the latest WindowsApplicationDriver-*.msi" -ForegroundColor Cyan
        Write-Host "3. Right-click the MSI file and select 'Install' (Run as Administrator)" -ForegroundColor Cyan
        return $false
    }
}

# Function to start WinAppDriver
function Start-WinAppDriverService {
    param([string]$Path)
    
    Write-Host "`nStarting WinAppDriver..." -ForegroundColor Yellow
    
    # Check if already running
    if (Test-WinAppDriverRunning) {
        Write-Host "WinAppDriver is already running" -ForegroundColor Yellow
        return $true
    }
    
    # Check if installed
    if (-not (Test-Path $Path)) {
        Write-Error "WinAppDriver not found at: $Path`nRun with -Install to install it first."
        return $false
    }
    
    # Check Developer Mode
    if (-not (Test-DeveloperMode)) {
        Write-Error "Developer Mode must be enabled to run WinAppDriver.`nPlease enable it in Windows Settings and restart if needed."
        return $false
    }
    
    try {
        # Start WinAppDriver
        $process = Start-Process -FilePath $Path -PassThru -WindowStyle Hidden
        Write-Host "WinAppDriver started (PID: $($process.Id))" -ForegroundColor Green
        
        # Wait for WinAppDriver to be ready
        Write-Host "Waiting for WinAppDriver to be ready..." -ForegroundColor Yellow
        $maxRetries = 15
        $retryCount = 0
        $ready = $false
        
        while ($retryCount -lt $maxRetries -and -not $ready) {
            Start-Sleep -Seconds 1
            $retryCount++
            Write-Host "  Checking... ($retryCount/$maxRetries)" -ForegroundColor Gray
            $ready = Test-WinAppDriverRunning
        }
        
        if ($ready) {
            Write-Host "OK: WinAppDriver is ready!" -ForegroundColor Green
            return $true
        } else {
            Write-Warning "WinAppDriver may not be fully ready, but continuing..."
            return $true
        }
    } catch {
        Write-Error "Failed to start WinAppDriver: $_"
        return $false
    }
}

# Main execution
$allChecksPassed = $true

# 1. Check/Install WinAppDriver
Write-Host "`n[1/4] Checking WinAppDriver..." -ForegroundColor Cyan
if (-not (Test-WinAppDriverInstalled -Path $WinAppDriverPath)) {
    if ($Install) {
        $allChecksPassed = Install-WinAppDriver -Path $WinAppDriverPath -and $allChecksPassed
    } else {
        Write-Host "  Run with -Install to automatically install WinAppDriver" -ForegroundColor Yellow
        $allChecksPassed = $false
    }
}

# 2. Check Developer Mode
Write-Host "`n[2/4] Checking Developer Mode..." -ForegroundColor Cyan
if (-not (Test-DeveloperMode)) {
    Write-Host "  Please enable Developer Mode manually in Windows Settings" -ForegroundColor Yellow
    $allChecksPassed = $false
}

# 3. Check App Build
Write-Host "`n[3/4] Checking App Build..." -ForegroundColor Cyan
$appBuilt, $appPath = Test-AppBuilt -RootDir $rootDir -Config $Configuration
if (-not $appBuilt) {
    Write-Host "  Building app..." -ForegroundColor Yellow
    $buildExitCode = 0
    try {
        Push-Location $rootDir
        dotnet build src/IdeaBranch.App/IdeaBranch.App.csproj -c $Configuration -f net9.0-windows10.0.19041.0
        $buildExitCode = $LASTEXITCODE
    } catch {
        Write-Error "Build failed: $_"
        $buildExitCode = 1
    } finally {
        Pop-Location
    }
    
    if ($buildExitCode -eq 0) {
        Write-Host "OK: App built successfully" -ForegroundColor Green
        $appBuilt = $true
    } else {
        Write-Error "App build failed. Please build manually."
        $allChecksPassed = $false
    }
}

if ($appBuilt) {
    # Set environment variable
    $env:MAUI_APP_PATH = $appPath
    $env:WINAPPDRIVER_PATH = $WinAppDriverPath
    Write-Host "  Environment variables set:" -ForegroundColor Gray
    Write-Host "    MAUI_APP_PATH = $appPath" -ForegroundColor Gray
    Write-Host "    WINAPPDRIVER_PATH = $WinAppDriverPath" -ForegroundColor Gray
}

# 4. Start WinAppDriver
Write-Host "`n[4/4] Checking WinAppDriver Service..." -ForegroundColor Cyan
if ($Start) {
    $allChecksPassed = Start-WinAppDriverService -Path $WinAppDriverPath -and $allChecksPassed
} else {
    if (-not (Test-WinAppDriverRunning)) {
        Write-Host "  WinAppDriver is not running" -ForegroundColor Yellow
        Write-Host "  Run with -Start to automatically start WinAppDriver" -ForegroundColor Yellow
        $allChecksPassed = $false
    }
}

# Verify everything
if ($Verify) {
    Write-Host "`n=========================================" -ForegroundColor Cyan
    Write-Host "Verification Summary" -ForegroundColor Cyan
    Write-Host "=========================================" -ForegroundColor Cyan
    
    $verifyPassed = $true
    $verifyPassed = (Test-WinAppDriverInstalled -Path $WinAppDriverPath) -and $verifyPassed
    $verifyPassed = (Test-DeveloperMode) -and $verifyPassed
    $appBuilt, $appPath = Test-AppBuilt -RootDir $rootDir -Config $Configuration
    $verifyPassed = $appBuilt -and $verifyPassed
    $verifyPassed = (Test-WinAppDriverRunning) -and $verifyPassed
    
    if ($verifyPassed) {
        Write-Host "`nOK: All checks passed! You're ready to run UI tests." -ForegroundColor Green
        Write-Host "`nTo run UI tests:" -ForegroundColor Yellow
        Write-Host "  `$env:ENABLE_UI_TESTS = '1'" -ForegroundColor Cyan
        Write-Host "  dotnet test tests/IdeaBranch.UITests/IdeaBranch.UITests.csproj -c $Configuration" -ForegroundColor Cyan
    } else {
        Write-Host "`nFAILED: Some checks failed. Please resolve the issues above." -ForegroundColor Red
    }
}

# Final summary
Write-Host "`n=========================================" -ForegroundColor Cyan
if ($allChecksPassed) {
    Write-Host "Setup completed successfully!" -ForegroundColor Green
} else {
    Write-Host "Setup completed with warnings" -ForegroundColor Yellow
    Write-Host "Run with -Verify to check status" -ForegroundColor Yellow
}
Write-Host "=========================================" -ForegroundColor Cyan

Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "1. Enable UI tests: `$env:ENABLE_UI_TESTS = `"1`"" -ForegroundColor Cyan
Write-Host "2. Run tests: dotnet test tests/IdeaBranch.UITests/IdeaBranch.UITests.csproj -c $Configuration" -ForegroundColor Cyan
Write-Host "   Or use: .\tests\IdeaBranch.UITests\scripts\run-ui-tests.ps1 -Configuration $Configuration" -ForegroundColor Cyan

