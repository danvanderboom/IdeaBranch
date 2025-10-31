param(
    [string]$LmStudioPath,
    [string]$ModelPath,
    [int]$Port = 1234,
    [int]$TimeoutSeconds = 60
)

function Test-LMStudioRunning {
    param([int]$Port)
    
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:$Port/v1/models" -Method GET -TimeoutSec 5 -ErrorAction Stop
        return $true
    }
    catch {
        return $false
    }
}

function Start-LMStudioProcess {
    param(
        [string]$Path,
        [string]$ModelPath,
        [int]$Port
    )
    
    $arguments = @()
    
    if ($ModelPath) {
        $arguments += "--model", $ModelPath
    }
    
    $arguments += "--port", $Port
    
    Write-Host "Starting LM Studio with arguments: $($arguments -join ' ')" -ForegroundColor Cyan
    
    try {
        $process = Start-Process -FilePath $Path -ArgumentList $arguments -PassThru -NoNewWindow
        return $process
    }
    catch {
        Write-Error "Failed to start LM Studio: $($_.Exception.Message)"
        return $null
    }
}

function Wait-ForLMStudio {
    param(
        [int]$Port,
        [int]$TimeoutSeconds
    )
    
    Write-Host "Waiting for LM Studio to be ready on port $Port..." -ForegroundColor Yellow
    
    $elapsed = 0
    while ($elapsed -lt $TimeoutSeconds) {
        if (Test-LMStudioRunning -Port $Port) {
            Write-Host "LM Studio is ready!" -ForegroundColor Green
            return $true
        }
        
        Start-Sleep -Seconds 2
        $elapsed += 2
        Write-Host "." -NoNewline
    }
    
    Write-Host ""
    Write-Error "LM Studio failed to start within $TimeoutSeconds seconds"
    return $false
}

# Main execution
Write-Host "LM Studio Management Script" -ForegroundColor Cyan

# Try to find LM Studio if path not provided
if (-not $LmStudioPath) {
    $possiblePaths = @(
        "${env:ProgramFiles}\LM Studio\LM Studio.exe",
        "${env:ProgramFiles(x86)}\LM Studio\LM Studio.exe",
        "${env:LOCALAPPDATA}\Programs\LM Studio\LM Studio.exe",
        "${env:USERPROFILE}\AppData\Local\Programs\LM Studio\LM Studio.exe"
    )
    
    foreach ($path in $possiblePaths) {
        if (Test-Path $path) {
            $LmStudioPath = $path
            Write-Host "Found LM Studio at: $LmStudioPath" -ForegroundColor Green
            break
        }
    }
    
    if (-not $LmStudioPath) {
        Write-Error "LM Studio not found. Please specify -LmStudioPath parameter."
        exit 1
    }
}

# Check if already running
if (Test-LMStudioRunning -Port $Port) {
    Write-Host "LM Studio is already running on port $Port" -ForegroundColor Green
    exit 0
}

# Start LM Studio
$process = Start-LMStudioProcess -Path $LmStudioPath -ModelPath $ModelPath -Port $Port
if (-not $process) {
    exit 1
}

# Wait for it to be ready
if (Wait-ForLMStudio -Port $Port -TimeoutSeconds $TimeoutSeconds) {
    Write-Host "LM Studio started successfully!" -ForegroundColor Green
    exit 0
} else {
    Write-Error "Failed to start LM Studio"
    exit 1
}

