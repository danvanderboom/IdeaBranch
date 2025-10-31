param(
    [ValidateSet("azure","lmstudio")]
    [string]$Provider = "azure",

    [string]$TestProject = "CriticalInsight.Data.UnitTests/CriticalInsight.Data.UnitTests.csproj",

    [int]$Trials = 20,
    [double]$Temperature = 0.2,
    [double]$TargetSuccess = 0.9,

    # Optional extra filter expression combined with AgentFrameworkLive
    [string]$Filter,

    # Azure settings
    [string]$AzureEndpoint,     # e.g. https://<resource>.openai.azure.com/openai/v1
    [string]$AzureDeployment,
    [string]$AzureApiKey,

    # LM Studio settings
    [string]$LmEndpoint = "http://localhost:1234/v1",
    [string]$LmModel,
    [string]$LmApiKey = "lm-studio",

    [switch]$NoBuild
)

function Set-Env($name, $value) {
    if ($null -ne $value -and $value -ne "") {
        Set-Item -Path "env:$name" -Value "$value"
    }
}

Write-Host "Running Agent Framework integration tests with provider '$Provider'" -ForegroundColor Cyan

# Common env
Set-Env CI_AF_LIVE 1
Set-Env CI_AF_PROVIDER $Provider
Set-Env CI_AF_TRIALS $Trials
Set-Env CI_AF_TEMP $Temperature
Set-Env CI_AF_TARGET_SUCCESS $TargetSuccess

if ($Provider -eq "azure") {
    if (-not $AzureEndpoint) { throw "AzureEndpoint is required for provider=azure" }
    if (-not $AzureDeployment) { throw "AzureDeployment is required for provider=azure" }
    Set-Env AZURE_OPENAI_ENDPOINT $AzureEndpoint
    Set-Env AZURE_OPENAI_DEPLOYMENT $AzureDeployment
    Set-Env AZURE_OPENAI_API_KEY $AzureApiKey
}
elseif ($Provider -eq "lmstudio") {
    if (-not $LmModel) { throw "LmModel is required for provider=lmstudio" }
    Set-Env LMSTUDIO_ENDPOINT $LmEndpoint
    Set-Env LMSTUDIO_MODEL $LmModel
    Set-Env LMSTUDIO_API_KEY $LmApiKey
}

$category = 'TestCategory=AgentFrameworkLive'
if ($Filter) {
    $filterExpr = "$category&($Filter)"
} else {
    $filterExpr = $category
}

$args = @('test', $TestProject, '--filter', $filterExpr)
if ($NoBuild) { $args += '--no-build' }

Write-Host "dotnet $($args -join ' ')" -ForegroundColor DarkGray
dotnet @args

exit $LASTEXITCODE


