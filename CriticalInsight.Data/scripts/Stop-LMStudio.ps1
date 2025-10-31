param(
    [int]$Port = 1234,
    [switch]$Force
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

function Stop-LMStudioProcess {
    param([int]$Port)
    
    # Try to find LM Studio processes
    $processes = Get-Process | Where-Object { $_.ProcessName -like "*lmstudio*" -or $_.ProcessName -like "*LM Studio*" }
    
    if ($processes) {
        Write-Host "Found LM Studio processes: $($processes.ProcessName -join ', ')" -ForegroundColor Yellow
        
        foreach ($process in $processes) {
            try {
                Write-Host "Stopping process: $($process.ProcessName) (PID: $($process.Id))" -ForegroundColor Cyan
                Stop-Process -Id $process.Id -Force
            }
            catch {
                Write-Warning "Failed to stop process $($process.ProcessName): $($_.Exception.Message)"
            }
        }
        
        # Wait a moment for processes to stop
        Start-Sleep -Seconds 3
        
        # Verify they're stopped
        $remainingProcesses = Get-Process | Where-Object { $_.ProcessName -like "*lmstudio*" -or $_.ProcessName -like "*LM Studio*" }
        if ($remainingProcesses) {
            Write-Warning "Some LM Studio processes may still be running"
            return $false
        } else {
            Write-Host "All LM Studio processes stopped successfully" -ForegroundColor Green
            return $true
        }
    } else {
        Write-Host "No LM Studio processes found" -ForegroundColor Yellow
        return $true
    }
}

# Main execution
Write-Host "LM Studio Stop Script" -ForegroundColor Cyan

# Check if LM Studio is running
if (-not (Test-LMStudioRunning -Port $Port)) {
    Write-Host "LM Studio is not running on port $Port" -ForegroundColor Yellow
    exit 0
}

if (-not $Force) {
    $confirm = Read-Host "Are you sure you want to stop LM Studio? (y/N)"
    if ($confirm -notmatch '^[yY]') {
        Write-Host "Operation cancelled" -ForegroundColor Yellow
        exit 0
    }
}

# Stop LM Studio
if (Stop-LMStudioProcess -Port $Port) {
    Write-Host "LM Studio stopped successfully!" -ForegroundColor Green
    exit 0
} else {
    Write-Error "Failed to stop LM Studio completely"
    exit 1
}

