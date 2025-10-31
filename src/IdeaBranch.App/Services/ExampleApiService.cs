using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace IdeaBranch.App.Services;

/// <summary>
/// Example API service demonstrating resilience policies with real HTTP calls.
/// Uses httpbin.org for testing retry and circuit breaker behavior.
/// </summary>
public sealed class ExampleApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExampleApiService> _logger;

    public ExampleApiService(IHttpClientFactory httpClientFactory, ILogger<ExampleApiService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("ExampleApi");
        _logger = logger;
    }

    /// <summary>
    /// Gets a test resource with automatic retry on transient errors.
    /// Demonstrates idempotent operation (GET) with full resilience policy.
    /// </summary>
    public async Task<TestResponse?> GetTestDataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Making GET request to /get endpoint");
            
            // httpbin.org/get returns request metadata - safe to retry
            var response = await _httpClient.GetAsync("/get", cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var data = await response.Content.ReadFromJsonAsync<TestResponse>(cancellationToken);
            
            _logger.LogInformation("GET request succeeded");
            return data;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "GET request failed after all retries");
            throw;
        }
    }

    /// <summary>
    /// Simulates a POST operation with limited retry (non-idempotent).
    /// Demonstrates that POST requests only retry on connection/timeout errors.
    /// </summary>
    public async Task<TestResponse?> PostTestDataAsync(object data, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Making POST request to /post endpoint");
            
            // httpbin.org/post echoes back the request - safe for testing
            var response = await _httpClient.PostAsJsonAsync("/post", data, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<TestResponse>(cancellationToken);
            
            _logger.LogInformation("POST request succeeded");
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "POST request failed");
            throw;
        }
    }

    /// <summary>
    /// Tests retry behavior with a status code endpoint.
    /// httpbin.org/status/{code} returns the specified status code.
    /// </summary>
    public async Task<bool> TestRetryBehaviorAsync(int statusCode, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Testing retry behavior with status code {StatusCode}", statusCode);
            
            var response = await _httpClient.GetAsync($"/status/{statusCode}", cancellationToken);
            
            // Transient errors (5xx) should retry and eventually succeed if service recovers
            // Non-transient errors (4xx) should not retry
            response.EnsureSuccessStatusCode();
            
            _logger.LogInformation("Request succeeded after potential retries");
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Request failed with status code {StatusCode}", statusCode);
            return false;
        }
    }

    /// <summary>
    /// Tests circuit breaker behavior by triggering multiple failures.
    /// Use /status/500 to trigger circuit breaker.
    /// </summary>
    public async Task<int> TestCircuitBreakerAsync(int numberOfAttempts, CancellationToken cancellationToken = default)
    {
        var successCount = 0;
        var failureCount = 0;
        var circuitBrokenCount = 0;

        _logger.LogInformation("Testing circuit breaker with {Count} attempts", numberOfAttempts);

        for (int i = 0; i < numberOfAttempts; i++)
        {
            try
            {
                var response = await _httpClient.GetAsync("/status/500", cancellationToken);
                response.EnsureSuccessStatusCode();
                successCount++;
            }
            catch (Polly.CircuitBreaker.BrokenCircuitException<HttpResponseMessage>)
            {
                circuitBrokenCount++;
                _logger.LogWarning("Circuit breaker opened - request rejected");
            }
            catch (HttpRequestException)
            {
                failureCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unexpected error on attempt {Attempt}", i + 1);
                failureCount++;
            }

            // Small delay between attempts
            await Task.Delay(100, cancellationToken);
        }

        _logger.LogInformation(
            "Circuit breaker test complete: {Success} successes, {Failures} failures, {Broken} circuit broken",
            successCount, failureCount, circuitBrokenCount);

        return circuitBrokenCount;
    }

    /// <summary>
    /// Tests delay endpoint to see exponential backoff in action.
    /// httpbin.org/delay/{seconds} waits before responding.
    /// </summary>
    public async Task<bool> TestDelayAsync(int seconds, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Testing delay endpoint with {Seconds}s delay", seconds);
            
            // Use a shorter timeout to see retry behavior
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(2)); // Shorter timeout to trigger retries
            
            var response = await _httpClient.GetAsync($"/delay/{seconds}", cts.Token);
            response.EnsureSuccessStatusCode();
            
            _logger.LogInformation("Delay test succeeded");
            return true;
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Request timed out (expected for delay test)");
            return false;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Delay test failed");
            return false;
        }
    }
}

/// <summary>
/// Test response model for httpbin.org endpoints.
/// </summary>
public sealed class TestResponse
{
    public Dictionary<string, string>? Args { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
    public string? Origin { get; set; }
    public string? Url { get; set; }
    public object? Json { get; set; }
}

