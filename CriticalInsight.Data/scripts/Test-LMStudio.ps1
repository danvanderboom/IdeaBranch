param(
    [string]$Endpoint = "http://localhost:1234/v1",
    [string]$ApiKey = "lm-studio"
)

function Invoke-Models($baseUrl, $key) {
    $url = ($baseUrl.TrimEnd('/')) + '/models'
    $headers = @{ 'Authorization' = "Bearer $key" }
    try {
        $resp = Invoke-RestMethod -Method GET -Uri $url -Headers $headers -TimeoutSec 10
        return $resp
    }
    catch {
        Write-Error "Failed to query $url : $($_.Exception.Message)"
        throw
    }
}

Write-Host "Querying LM Studio models at $Endpoint" -ForegroundColor Cyan
$resp = Invoke-Models -baseUrl $Endpoint -key $ApiKey
if ($null -eq $resp -or $null -eq $resp.data) {
    Write-Error "Unexpected response."
    exit 2
}

Write-Host "Models:" -ForegroundColor Green
foreach ($m in $resp.data) {
    Write-Host " - $($m.id)"
}

exit 0


