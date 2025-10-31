param(
    [string]$Endpoint = "http://localhost:1234/v1",

    [Parameter(Mandatory=$true)]
    [string]$Model,

    [string]$ApiKey = "lm-studio",

    [int]$Trials = 20,
    [double]$Temperature = 0.2,
    [double]$TargetSuccess = 0.9,
    [switch]$NoBuild,
    
    # LM Studio management options
    [string]$LmStudioPath,
    [string]$ModelPath,
    [switch]$AutoStart,
    [switch]$AutoStop,
    [int]$StartTimeoutSeconds = 60,
    [int]$ModelTimeoutSeconds = 30
)

# Extract port from endpoint
$port = 1234
if ($Endpoint -match ':(\d+)') {
    $port = [int]$matches[1]
}

# Auto-start LM Studio if requested
if ($AutoStart) {
    Write-Host "Auto-starting LM Studio..." -ForegroundColor Cyan
    
    $manageArgs = @{
        Port = $port
        StartTimeoutSeconds = $StartTimeoutSeconds
        ModelTimeoutSeconds = $ModelTimeoutSeconds
    }
    
    if ($LmStudioPath) { $manageArgs.LmStudioPath = $LmStudioPath }
    if ($ModelPath) { $manageArgs.ModelPath = $ModelPath }
    if ($AutoStop) { $manageArgs.AutoStop = $true }
    
    & "$PSScriptRoot/Manage-LMStudio.ps1" @manageArgs
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to start LM Studio"
        exit $LASTEXITCODE
    }
}

# Run the tests
& "$PSScriptRoot/Run-AgentTests.ps1" `
    -Provider lmstudio `
    -LmEndpoint $Endpoint `
    -LmModel $Model `
    -LmApiKey $ApiKey `
    -Trials $Trials `
    -Temperature $Temperature `
    -TargetSuccess $TargetSuccess `
    -NoBuild:$NoBuild

$testExitCode = $LASTEXITCODE

# Auto-stop LM Studio if requested
if ($AutoStop) {
    Write-Host "Auto-stopping LM Studio..." -ForegroundColor Cyan
    & "$PSScriptRoot/Stop-LMStudio.ps1" -Port $port -Force
}

exit $testExitCode


