param(
    [int]$Port = 1234,
    [string]$ModelPath,
    [string]$LmStudioPath,
    [int]$StartTimeoutSeconds = 60,
    [int]$ModelTimeoutSeconds = 30,
    [switch]$AutoStop
)

function Test-LMStudioRunning {
    param([int]$Port)
    
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:$Port/v1/models" -Method GET -TimeoutSec 5 -ErrorAction Stop
        return $true, $response
    }
    catch {
        return $false, $null
    }
}

function Start-LMStudioIfNeeded {
    param(
        [int]$Port,
        [string]$ModelPath,
        [string]$LmStudioPath
    )
    
    $isRunning, $response = Test-LMStudioRunning -Port $Port
    
    if ($isRunning) {
        Write-Host "LM Studio is already running on port $Port" -ForegroundColor Green
        return $true
    }
    
    Write-Host "Starting LM Studio..." -ForegroundColor Cyan
    
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
            Write-Error "LM Studio not found. Please specify -LmStudioPath parameter or install LM Studio."
            return $false
        }
    }
    
    # Start LM Studio
    $arguments = @()
    if ($ModelPath) {
        $arguments += "--model", $ModelPath
    }
    $arguments += "--port", $Port
    
    try {
        Write-Host "Starting LM Studio with arguments: $($arguments -join ' ')" -ForegroundColor Cyan
        $process = Start-Process -FilePath $LmStudioPath -ArgumentList $arguments -PassThru -NoNewWindow
    }
    catch {
        Write-Error "Failed to start LM Studio: $($_.Exception.Message)"
        return $false
    }
    
    # Wait for LM Studio to be ready
    Write-Host "Waiting for LM Studio to be ready..." -ForegroundColor Yellow
    $elapsed = 0
    while ($elapsed -lt $StartTimeoutSeconds) {
        $isRunning, $response = Test-LMStudioRunning -Port $Port
        if ($isRunning) {
            Write-Host ""
            Write-Host "LM Studio is ready!" -ForegroundColor Green
            return $true
        }
        
        Start-Sleep -Seconds 2
        $elapsed += 2
        Write-Host "." -NoNewline
    }
    
    Write-Host ""
    Write-Error "LM Studio failed to start within $StartTimeoutSeconds seconds"
    return $false
}

function Wait-ForModel {
    param(
        [int]$Port,
        [string]$ExpectedModel,
        [int]$TimeoutSeconds
    )
    
    if (-not $ExpectedModel) {
        return $true
    }
    
    Write-Host "Waiting for model '$ExpectedModel' to be ready..." -ForegroundColor Yellow
    
    $elapsed = 0
    while ($elapsed -lt $TimeoutSeconds) {
        $isRunning, $response = Test-LMStudioRunning -Port $Port
        
        if ($isRunning -and $response -and $response.data) {
            $loadedModels = $response.data | ForEach-Object { $_.id }
            
            if ($loadedModels -contains $ExpectedModel) {
                Write-Host ""
                Write-Host "Model '$ExpectedModel' is ready!" -ForegroundColor Green
                return $true
            }
        }
        
        Start-Sleep -Seconds 2
        $elapsed += 2
        Write-Host "." -NoNewline
    }
    
    Write-Host ""
    Write-Warning "Model '$ExpectedModel' not ready within $TimeoutSeconds seconds"
    return $false
}

function Stop-LMStudio {
    param([int]$Port)
    
    Write-Host "Stopping LM Studio..." -ForegroundColor Cyan
    
    $processes = Get-Process | Where-Object { $_.ProcessName -like "*lmstudio*" -or $_.ProcessName -like "*LM Studio*" }
    
    if ($processes) {
        foreach ($process in $processes) {
            try {
                Write-Host "Stopping process: $($process.ProcessName) (PID: $($process.Id))" -ForegroundColor Cyan
                Stop-Process -Id $process.Id -Force
            }
            catch {
                Write-Warning "Failed to stop process $($process.ProcessName): $($_.Exception.Message)"
            }
        }
        
        Start-Sleep -Seconds 3
        Write-Host "LM Studio stopped" -ForegroundColor Green
    } else {
        Write-Host "No LM Studio processes found" -ForegroundColor Yellow
    }
}

# Main execution
Write-Host "LM Studio Management Script" -ForegroundColor Cyan

try {
    # Start LM Studio if needed
    if (-not (Start-LMStudioIfNeeded -Port $Port -ModelPath $ModelPath -LmStudioPath $LmStudioPath)) {
        exit 1
    }
    
    # Wait for specific model if provided
    if ($ModelPath) {
        $modelName = [System.IO.Path]::GetFileNameWithoutExtension($ModelPath)
        if (-not (Wait-ForModel -Port $Port -ExpectedModel $modelName -TimeoutSeconds $ModelTimeoutSeconds)) {
            Write-Warning "Model may not be ready, but continuing..."
        }
    }
    
    Write-Host "LM Studio is ready for testing!" -ForegroundColor Green
    
    # If AutoStop is requested, stop LM Studio after a delay
    if ($AutoStop) {
        Write-Host "AutoStop enabled - LM Studio will stop after tests complete" -ForegroundColor Yellow
        # Note: In a real scenario, you'd want to hook into test completion events
        # For now, this is just a placeholder
    }
    
    exit 0
}
catch {
    Write-Error "Unexpected error: $($_.Exception.Message)"
    exit 1
}

