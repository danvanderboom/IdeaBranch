# Resilience Policies with Polly

This guide explains how to use Polly-based resilience policies in .NET MAUI applications for handling transient failures in outbound I/O operations (HTTP, storage, messaging).

## Overview

The resilience policy system provides:
- **Exponential backoff with decorrelated jitter** for retries
- **Circuit breaker** protection against cascading failures
- **Automatic policy selection** based on HTTP method (idempotent vs non-idempotent)
- **Telemetry and logging** for retry attempts, circuit breaker state, and outcomes

## Quick Start

Resilience policies are automatically configured when you call `AddResiliencePolicies()` in your `MauiProgram.cs`:

```csharp
builder.Services.AddResiliencePolicies();
```

After this, you need to configure named `HttpClient` instances with the standard resilience policy. Any named `HttpClient` configured with `AddStandardResiliencePolicy()` will automatically use the appropriate resilience policy based on the HTTP method.

## Usage Patterns

### Basic HttpClient Usage

First, configure a named `HttpClient` in `MauiProgram.cs`:

```csharp
// In MauiProgram.cs
builder.Services.AddHttpClient("MyApi")
    .AddStandardResiliencePolicy("MyApi")
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri("https://api.example.com");
    });
```

Then inject `IHttpClientFactory` and use the named client:

```csharp
public class MyApiService
{
    private readonly HttpClient _httpClient;

    public MyApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("MyApi");
    }

    public async Task<MyData> GetDataAsync(CancellationToken cancellationToken = default)
    {
        // GET requests automatically use idempotent policy (retry + circuit breaker)
        var response = await _httpClient.GetAsync("/data", cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var data = await response.Content.ReadFromJsonAsync<MyData>(cancellationToken);
        return data!;
    }

    public async Task<MyData> PostDataAsync(MyData data, CancellationToken cancellationToken = default)
    {
        // POST requests automatically use non-idempotent policy (no retries by default)
        var response = await _httpClient.PostAsJsonAsync("/data", data, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<MyData>(cancellationToken);
        return result!;
    }
}
```

### Named HttpClient Configuration

For more control, configure a named `HttpClient` with specific settings:

```csharp
// In MauiProgram.cs
builder.Services.AddHttpClient("MyApi")
    .AddStandardResiliencePolicy("MyApi")
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri("https://api.example.com");
        client.Timeout = TimeSpan.FromSeconds(30);
    });

// In your service
public class MyApiService
{
    private readonly HttpClient _httpClient;

    public MyApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("MyApi");
    }
    
    // ... use _httpClient as before
}
```

### Custom Policy Configuration

For advanced scenarios, you can access the policy registry directly:

```csharp
public class MyService
{
    private readonly ResiliencePolicyRegistry _policyRegistry;
    private readonly HttpClient _httpClient;

    public MyService(
        IHttpClientFactory httpClientFactory,
        ResiliencePolicyRegistry policyRegistry)
    {
        _policyRegistry = policyRegistry;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<MyData> ExecuteWithCustomPolicyAsync(CancellationToken cancellationToken = default)
    {
        var policy = _policyRegistry.GetStandardResiliencePolicy("CustomPolicy");
        
        var response = await policy.ExecuteAsync(async () =>
        {
            return await _httpClient.GetAsync("https://api.example.com/data", cancellationToken);
        });
        
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MyData>(cancellationToken) ?? throw new InvalidOperationException();
    }
}
```

## Policy Types

### Idempotent Operations (GET, PUT, DELETE, PATCH, HEAD, OPTIONS)

**Automatic retries with exponential backoff + jitter:**
- Retries up to 3 times
- Exponential backoff: 100ms, ~200ms, ~400ms (with jitter)
- Maximum delay capped at 30 seconds
- Circuit breaker: Opens after 5 consecutive failures, stays open for 30 seconds

**Transient errors that trigger retries:**
- HTTP 408 (Request Timeout)
- HTTP 429 (Too Many Requests)
- HTTP 5xx (except 501, 505)
- `HttpRequestException`
- `TaskCanceledException` (timeouts)

### Non-Idempotent Operations (POST)

**Limited retries:**
- Retries only on connection/timeout errors (not on HTTP errors)
- Single retry attempt with 500ms delay
- No circuit breaker (to avoid blocking legitimate requests)

**Why limited?** POST requests can have side effects (creating resources, sending emails, etc.). Retrying a failed POST that actually succeeded server-side could cause duplicates.

**When to use idempotent policies for POST:**
If your POST operations are idempotent (e.g., using idempotency keys), you can configure a custom policy:

```csharp
builder.Services.AddHttpClient("IdempotentApi")
    .AddResiliencePolicy(sp =>
    {
        var registry = sp.GetRequiredService<ResiliencePolicyRegistry>();
        return registry.GetIdempotentHttpPolicy("IdempotentApi");
    });
```

## Telemetry and Logging

All resilience operations emit telemetry events that are logged:

### Retry Events

```
[Resilience] Retry attempt 1 for policy 'StandardHttpRetry'. Delay: 156.32ms, Reason: 429, CorrelationId: abc123
```

### Circuit Breaker Events

**When circuit opens:**
```
[Resilience] Circuit breaker opened for policy 'StandardCircuitBreaker'. Duration: 30s, Reason: 500, CorrelationId: abc123
```

**When circuit resets:**
```
[Resilience] Circuit breaker reset for policy 'StandardCircuitBreaker'. CorrelationId: abc123
```

**When circuit is half-open (testing):**
```
[Resilience] Circuit breaker half-open for policy 'StandardCircuitBreaker'
```

### Log Levels

- **Information**: Retry attempts, circuit breaker state changes, successful operations
- **Warning**: Retry warnings, circuit breaker opened
- **Error**: Final failures after all retries exhausted

### Structured Telemetry

The `ResilienceTelemetryEmitter` class provides hooks for integrating with structured telemetry systems (Application Insights, OpenTelemetry, etc.). See `ResilienceTelemetryEmitter.cs` for implementation details.

## Best Practices

### 1. Use HttpClientFactory

Always create `HttpClient` instances through `IHttpClientFactory` to ensure proper policy application and resource management:

```csharp
// ✅ Good
var httpClient = httpClientFactory.CreateClient();

// ❌ Bad - bypasses policies
var httpClient = new HttpClient();
```

### 2. Cancellation Tokens

Always pass `CancellationToken` to async HTTP operations to support cancellation:

```csharp
await _httpClient.GetAsync("https://api.example.com/data", cancellationToken);
```

### 3. Idempotency Keys for POST

If you need retries for POST operations, use idempotency keys:

```csharp
public async Task<MyData> CreateWithIdempotencyAsync(
    MyData data,
    string idempotencyKey,
    CancellationToken cancellationToken = default)
{
    var request = new HttpRequestMessage(HttpMethod.Post, "https://api.example.com/data")
    {
        Content = JsonContent.Create(data)
    };
    request.Headers.Add("Idempotency-Key", idempotencyKey);
    
    var response = await _httpClient.SendAsync(request, cancellationToken);
    // ... handle response
}
```

Then configure that client with an idempotent policy (see "Custom Policy Configuration" above).

### 4. Handle Timeouts Appropriately

Configure appropriate timeouts based on your use case:

```csharp
builder.Services.AddHttpClient("MyApi")
    .AddStandardResiliencePolicy()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });
```

### 5. Monitor Circuit Breaker State

Watch for circuit breaker events in your logs to detect upstream service issues early. Consider alerting on repeated circuit breaker activations.

## Advanced Configuration

### Custom Retry Count

Access the registry directly and build custom policies:

```csharp
public class CustomService
{
    private readonly ResiliencePolicyRegistry _registry;

    public async Task ExecuteWithCustomRetryAsync()
    {
        // Build a custom retry policy with more attempts
        var retryPolicy = Policy<HttpResponseMessage>
            .HandleResult(response => response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: (retryAttempt, result, context) =>
                {
                    // Custom backoff logic
                    return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                });
        
        // Use with your HttpClient...
    }
}
```

### Per-Request Policy Selection

You can override policy selection per request using Polly's `Context`:

```csharp
var context = new Context { ["PolicyName"] = "CustomPolicy" };
var response = await policy.ExecuteAsync(async ctx =>
{
    return await _httpClient.GetAsync("https://api.example.com/data");
}, context);
```

## Troubleshooting

### Policy Not Applied

**Issue:** Retries not happening

**Solution:** Ensure you're using `IHttpClientFactory.CreateClient()`, not `new HttpClient()`.

### Too Many Retries

**Issue:** Requests taking too long due to retries

**Solution:** Review your timeout settings. Retries respect timeouts and will fail fast if the total time exceeds the timeout.

### Circuit Breaker Not Opening

**Issue:** Circuit breaker not activating when expected

**Solution:** Check the threshold (default: 5 consecutive failures). Ensure failures are transient errors that the policy handles (see "Policy Types" above).

## See Also

- [Polly Documentation](https://www.pollydocs.org/)
- [HttpClientFactory Best Practices](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests)
- `ResiliencePolicyRegistry.cs` - Policy registry implementation
- `ResilienceTelemetryEmitter.cs` - Telemetry emission implementation

