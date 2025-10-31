param(
    [int]$Port = 1234,
    [string]$ExpectedModel,
    [int]$TimeoutSeconds = 30
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

function Wait-ForModel {
    param(
        [int]$Port,
        [string]$ExpectedModel,
        [int]$TimeoutSeconds
    )
    
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
            
            Write-Host "Available models: $($loadedModels -join ', ')" -ForegroundColor Gray
        }
        
        Start-Sleep -Seconds 2
        $elapsed += 2
        Write-Host "." -NoNewline
    }
    
    Write-Host ""
    Write-Warning "Model '$ExpectedModel' not ready within $TimeoutSeconds seconds"
    return $false
}

# Main execution
Write-Host "LM Studio Model Check Script" -ForegroundColor Cyan

# Check if LM Studio is running
$isRunning, $response = Test-LMStudioRunning -Port $Port

if (-not $isRunning) {
    Write-Error "LM Studio is not running on port $Port"
    exit 1
}

Write-Host "LM Studio is running on port $Port" -ForegroundColor Green

# Show current models
if ($response -and $response.data) {
    Write-Host "Current models:" -ForegroundColor Cyan
    foreach ($model in $response.data) {
        Write-Host "  - $($model.id)" -ForegroundColor White
    }
} else {
    Write-Host "No models currently loaded" -ForegroundColor Yellow
}

# Wait for specific model if requested
if ($ExpectedModel) {
    if (Wait-ForModel -Port $Port -ExpectedModel $ExpectedModel -TimeoutSeconds $TimeoutSeconds) {
        exit 0
    } else {
        exit 1
    }
} else {
    Write-Host "No specific model requested - just checking status" -ForegroundColor Yellow
    exit 0
}

