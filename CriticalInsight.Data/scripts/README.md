# Scripts for Agent Framework Integration Tests

PowerShell helpers to run live Agent Framework tests against Azure OpenAI or an OpenAI-compatible local server (LM Studio). Tests evaluate whether different hierarchical views improve multiple-choice accuracy.

## Prereqs
- PowerShell
- .NET SDK
- For Azure: Azure subscription and model deployment; optionally `az login`.
- For LM Studio: local server exposing OpenAI-compatible `/v1` API.

## Scripts

- `Run-AgentTests.ps1` — generic runner (parameterized)
- `Run-AgentTests-Azure.ps1` — convenience wrapper for Azure
- `Run-AgentTests-LMStudio.ps1` — convenience wrapper for LM Studio with auto-management
- `Test-LMStudio.ps1` — quick check to list models from LM Studio
- `Start-LMStudio.ps1` — start LM Studio server with optional model loading
- `Stop-LMStudio.ps1` — stop LM Studio server
- `Wait-LMStudioModel.ps1` — wait for specific model to be ready
- `Manage-LMStudio.ps1` — comprehensive LM Studio lifecycle management

## Usage

### Azure
```powershell
# With API key
./Run-AgentTests-Azure.ps1 -Endpoint "https://<resource>.openai.azure.com/openai/v1" -Deployment "<deploy>" -ApiKey "<key>" -Trials 20 -Temperature 0.2 -TargetSuccess 0.9

# Or with Azure CLI (omit -ApiKey)
az login
./Run-AgentTests-Azure.ps1 -Endpoint "https://<resource>.openai.azure.com/openai/v1" -Deployment "<deploy>"
```

### LM Studio (Manual Management)
```powershell
# Start LM Studio manually, then verify and run tests
./Start-LMStudio.ps1 -ModelPath "C:\path\to\model.gguf" -Port 1234
./Test-LMStudio.ps1 -Endpoint "http://localhost:1234/v1"
./Run-AgentTests-LMStudio.ps1 -Model "<model-id>" -Endpoint "http://localhost:1234/v1" -Trials 20 -Temperature 0.2 -TargetSuccess 0.9
./Stop-LMStudio.ps1 -Port 1234
```

### LM Studio (Auto Management)
```powershell
# Fully automated - starts LM Studio, runs tests, stops LM Studio
./Run-AgentTests-LMStudio.ps1 -Model "<model-id>" -ModelPath "C:\path\to\model.gguf" -AutoStart -AutoStop -Trials 20 -Temperature 0.2 -TargetSuccess 0.9

# Auto-start only (you stop manually)
./Run-AgentTests-LMStudio.ps1 -Model "<model-id>" -AutoStart -Trials 20

# Auto-stop only (you start manually)
./Run-AgentTests-LMStudio.ps1 -Model "<model-id>" -AutoStop -Trials 20
```

### LM Studio Management Scripts
```powershell
# Start LM Studio with specific model
./Start-LMStudio.ps1 -ModelPath "C:\path\to\model.gguf" -Port 1234 -TimeoutSeconds 60

# Check if LM Studio is running and what models are loaded
./Wait-LMStudioModel.ps1 -Port 1234 -ExpectedModel "my-model"

# Stop LM Studio
./Stop-LMStudio.ps1 -Port 1234 -Force

# Comprehensive management
./Manage-LMStudio.ps1 -Port 1234 -ModelPath "C:\path\to\model.gguf" -StartTimeoutSeconds 60 -ModelTimeoutSeconds 30
```

### Generic runner
```powershell
# Azure
./Run-AgentTests.ps1 -Provider azure -AzureEndpoint "https://<resource>.openai.azure.com/openai/v1" -AzureDeployment "<deploy>" -AzureApiKey "<key>" -Trials 30 -Temperature 0.1 -TargetSuccess 0.95

# LM Studio
./Run-AgentTests.ps1 -Provider lmstudio -LmEndpoint "http://localhost:1234/v1" -LmModel "<model-id>" -Trials 20 -Temperature 0.2 -TargetSuccess 0.9
```

## LM Studio Auto-Discovery

The scripts will automatically search for LM Studio in common installation locations:
- `%ProgramFiles%\LM Studio\LM Studio.exe`
- `%ProgramFiles(x86)%\LM Studio\LM Studio.exe`
- `%LOCALAPPDATA%\Programs\LM Studio\LM Studio.exe`
- `%USERPROFILE%\AppData\Local\Programs\LM Studio\LM Studio.exe`

If LM Studio is installed elsewhere, use the `-LmStudioPath` parameter.

## Provider-Aware Configuration

The integration tests automatically adjust behavior based on the provider:

### Azure OpenAI
- **Target Success Rate**: 90% (configurable via `CI_AF_TARGET_SUCCESS`)
- **Success Delta**: 30% minimum difference between good/bad views (via `CI_AF_DELTA`)
- **Trials**: 8 per scenario (via `CI_AF_TRIALS`)
- **Per-Call Timeout**: 5 seconds (via `CI_AF_PER_CALL_TIMEOUT_MS`)
- **Temperature**: 0.2 (via `CI_AF_TEMP`)

### LM Studio
- **Target Success Rate**: 60% (configurable via `CI_AF_TARGET_SUCCESS`)
- **Success Delta**: 20% minimum difference between good/bad views (via `CI_AF_DELTA`)
- **Trials**: 3 per scenario (via `CI_AF_TRIALS`)
- **Per-Call Timeout**: 12 seconds (via `CI_AF_PER_CALL_TIMEOUT_MS`)
- **Temperature**: 0.2 (via `CI_AF_TEMP`)

### Environment Variables
```powershell
# Override defaults
$env:CI_AF_TARGET_SUCCESS = "0.8"    # Target success rate
$env:CI_AF_DELTA = "0.25"            # Minimum good vs bad difference
$env:CI_AF_TRIALS = "5"              # Trials per scenario
$env:CI_AF_PER_CALL_TIMEOUT_MS = "8000"  # Per-call timeout
$env:CI_AF_TEMP = "0.1"              # Temperature
```

## CI Profiles

### Azure CI Profile
```powershell
# High-performance Azure configuration for CI
$env:CI_AF_TARGET_SUCCESS = "0.9"
$env:CI_AF_DELTA = "0.3"
$env:CI_AF_TRIALS = "8"
$env:CI_AF_PER_CALL_TIMEOUT_MS = "5000"
$env:CI_AF_TEMP = "0.2"

./Run-AgentTests-Azure.ps1 -Endpoint "https://<resource>.openai.azure.com/openai/v1" -Deployment "<deploy>" -ApiKey "<key>"
```

### LM Studio CI Profile
```powershell
# Conservative LM Studio configuration for CI
$env:CI_AF_TARGET_SUCCESS = "0.6"
$env:CI_AF_DELTA = "0.2"
$env:CI_AF_TRIALS = "3"
$env:CI_AF_PER_CALL_TIMEOUT_MS = "12000"
$env:CI_AF_TEMP = "0.2"

./Run-AgentTests-LMStudio.ps1 -Model "<model-id>" -ModelPath "C:\path\to\model.gguf" -AutoStart -AutoStop
```

### Development Profile
```powershell
# Quick iteration with fewer trials
$env:CI_AF_TRIALS = "2"
$env:CI_AF_PER_CALL_TIMEOUT_MS = "10000"

./Run-AgentTests-LMStudio.ps1 -Model "<model-id>" -AutoStart -AutoStop
```

## Notes
- Instructions constrain answers to a single letter to reduce variance.
- Views use `TreeViewJsonSerializer` with depth/property filters to manage context size.
- LM Studio management scripts handle process lifecycle and model loading.
- Auto-management features require LM Studio to support command-line arguments (varies by version).
- The scripts set `CI_AF_LIVE=1` and other env vars expected by the integration tests.
- Tests are filtered by `TestCategory=AgentFrameworkLive`.
- Provider-aware thresholds ensure appropriate expectations for each LLM capability level.