# Agent Framework Integration Tests

These NUnit tests run live calls using Microsoft Agent Framework against Azure OpenAI (primary) or a local OpenAI-compatible endpoint (LM Studio). Tests evaluate whether different hierarchical views improve multiple-choice accuracy.

## Running

Set the opt-in flag; otherwise tests are skipped:

```powershell
$env:CI_AF_LIVE = "1"
```

Choose a provider (default is `azure`):

```powershell
$env:CI_AF_PROVIDER = "azure" # or "lmstudio"
```

### Azure OpenAI

Either set an API key:

```powershell
$env:AZURE_OPENAI_ENDPOINT = "https://<resource>.openai.azure.com/openai/v1"
$env:AZURE_OPENAI_DEPLOYMENT = "<deployment-name>"
$env:AZURE_OPENAI_API_KEY = "<key>"
```

Or sign in with `az login` and omit the API key:

```powershell
az login
$env:AZURE_OPENAI_ENDPOINT = "https://<resource>.openai.azure.com/openai/v1"
$env:AZURE_OPENAI_DEPLOYMENT = "<deployment-name>"
```

### LM Studio

Start the local server (OpenAI-compatible) and set:

```powershell
$env:CI_AF_PROVIDER = "lmstudio"
$env:LMSTUDIO_ENDPOINT = "http://localhost:1234/v1"
$env:LMSTUDIO_MODEL = "<model-id>"
# optional
$env:LMSTUDIO_API_KEY = "lm-studio"
```

### Tuning

- Trials: `$env:CI_AF_TRIALS` (default 20)
- Temperature: `$env:CI_AF_TEMP` (default 0.2; provider dependent)
- Target success: `$env:CI_AF_TARGET_SUCCESS` (default 0.9)

## Notes
- Instructions constrain answers to a single letter to reduce variance.
- Views use `TreeViewJsonSerializer` with depth/property filters to manage context size.
