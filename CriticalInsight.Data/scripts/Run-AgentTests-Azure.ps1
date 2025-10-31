param(
    [Parameter(Mandatory=$true)]
    [string]$Endpoint,  # https://<resource>.openai.azure.com/openai/v1

    [Parameter(Mandatory=$true)]
    [string]$Deployment,

    [string]$ApiKey,

    [int]$Trials = 20,
    [double]$Temperature = 0.2,
    [double]$TargetSuccess = 0.9,
    [switch]$NoBuild
)

& "$PSScriptRoot/Run-AgentTests.ps1" `
    -Provider azure `
    -AzureEndpoint $Endpoint `
    -AzureDeployment $Deployment `
    -AzureApiKey $ApiKey `
    -Trials $Trials `
    -Temperature $Temperature `
    -TargetSuccess $TargetSuccess `
    -NoBuild:$NoBuild

exit $LASTEXITCODE


