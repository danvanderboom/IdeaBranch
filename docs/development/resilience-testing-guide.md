# Resilience Policies Testing Guide

This guide explains how to test resilience policies in the MAUI app with real API calls.

## Overview

The app includes a `ResilienceTestPage` that demonstrates resilience policies using [httpbin.org](https://httpbin.org), a free HTTP testing service. This allows you to see retry behavior, circuit breaker activation, and telemetry in action.

## Running the App

1. **Build the app:**
   ```bash
   dotnet build src/IdeaBranch.App/IdeaBranch.App.csproj
   ```

2. **Run the app (Windows):**
   ```bash
   dotnet run --project src/IdeaBranch.App/IdeaBranch.App.csproj
   ```

3. **Navigate to the Resilience Test page:**
   - In the app shell, select "Resilience Test" from the navigation

## Testing Scenarios

### 1. Test GET (Idempotent Retry)

**Purpose:** Demonstrates full retry behavior on idempotent operations.

**What to test:**
- Click "Test GET (Idempotent Retry)"
- This calls `httpbin.org/get` which returns request metadata
- Check Debug Output for `[Resilience]` logs showing retry attempts

**Expected behavior:**
- GET requests use the full retry policy (3 attempts with exponential backoff)
- On transient errors (408, 429, 5xx), the request will retry automatically
- Telemetry logs show each retry attempt with delay and reason

### 2. Test POST (Limited Retry)

**Purpose:** Demonstrates limited retry on non-idempotent operations.

**What to test:**
- Click "Test POST (Limited Retry)"
- This calls `httpbin.org/post` with test data
- Check Debug Output for telemetry logs

**Expected behavior:**
- POST requests use a limited retry policy (no retries on HTTP errors, only on timeouts)
- This prevents duplicate operations from being retried
- Telemetry logs show when retries occur (only on timeout/connection errors)

### 3. Test Retry (500 Error)

**Purpose:** Demonstrates exponential backoff with jitter on transient errors.

**What to test:**
- Click "Test Retry (500 Error)"
- This calls `httpbin.org/status/500` which always returns 500
- Watch Debug Output for retry attempts

**Expected behavior:**
- Request will retry up to 3 times with exponential backoff
- Delays increase: ~100ms, ~200ms, ~400ms (with jitter)
- Check logs for `[Resilience] Retry attempt` messages
- Each log shows the delay, reason, and correlation ID

**Debug Output Example:**
```
[Resilience] Retry attempt 1 for policy 'ExampleApi_Idempotent_Retry'. Delay: 156ms, Reason: InternalServerError
[Resilience] Retry attempt 2 for policy 'ExampleApi_Idempotent_Retry'. Delay: 287ms, Reason: InternalServerError
[Resilience] Retry attempt 3 for policy 'ExampleApi_Idempotent_Retry'. Delay: 523ms, Reason: InternalServerError
```

### 4. Test Circuit Breaker

**Purpose:** Demonstrates circuit breaker opening after consecutive failures.

**What to test:**
- Click "Test Circuit Breaker"
- This sends 10 requests to `httpbin.org/status/500` (all will fail)
- Watch Debug Output for circuit breaker activation

**Expected behavior:**
- After 5 consecutive failures, the circuit breaker opens
- Subsequent requests are immediately rejected with `BrokenCircuitException`
- Check logs for `[Resilience] Circuit breaker opened` message
- After 30 seconds, the circuit enters half-open state (testing mode)
- If the next request succeeds, the circuit closes; if it fails, it opens again

**Debug Output Example:**
```
[Resilience] Retry attempt 1 for policy 'ExampleApi_Idempotent_Retry'. Delay: 156ms, Reason: InternalServerError
...
[Resilience] Circuit breaker opened for policy 'ExampleApi_Idempotent_CircuitBreaker'. Duration: 30s, Reason: InternalServerError
[Resilience] Circuit breaker half-open for policy 'ExampleApi_Idempotent_CircuitBreaker'
```

### 5. Test Delay/Timeout

**Purpose:** Demonstrates timeout handling and retry behavior.

**What to test:**
- Click "Test Delay/Timeout"
- This calls `httpbin.org/delay/5` (5-second delay) with a 2-second timeout
- Watch Debug Output for timeout and retry behavior

**Expected behavior:**
- Request times out after 2 seconds
- On timeout (`TaskCanceledException`), the request may retry (depending on policy)
- For idempotent operations, timeouts trigger retries
- For non-idempotent operations, only connection errors trigger retries

## Observing Telemetry

All resilience operations emit telemetry logs. To see them:

1. **In Visual Studio:**
   - Open Output window (View → Output)
   - Select "Debug" from the "Show output from" dropdown
   - Look for messages starting with `[Resilience]`

2. **In Visual Studio Code:**
   - Open Debug Console
   - Logs appear in the console during app execution

3. **Log Levels:**
   - **Information**: Retry attempts, circuit breaker state changes
   - **Warning**: Retry warnings, circuit breaker opened
   - **Error**: Final failures after all retries exhausted
   - **Debug**: Successful operations (optional)

## Telemetry Log Format

All resilience telemetry follows this format:

```
[Resilience] <Event Type> for policy '<PolicyName>'. <Details>, CorrelationId: <CorrelationId>
```

**Event Types:**
- `Retry attempt <N>` - Retry attempt with delay and reason
- `Circuit breaker opened` - Circuit breaker activated
- `Circuit breaker reset` - Circuit breaker closed
- `Circuit breaker half-open` - Circuit breaker testing
- `Operation succeeded` - Successful operation
- `Operation failed` - Failed operation

## Testing with Different Status Codes

You can modify `ExampleApiService.TestRetryBehaviorAsync()` to test different scenarios:

- **500, 502, 503** - Transient errors (trigger retries)
- **408** - Request Timeout (triggers retries)
- **429** - Too Many Requests (triggers retries)
- **400, 401, 404** - Non-transient errors (no retries)
- **501, 505** - Special 5xx codes (no retries)

## Expected Behaviors Summary

| Operation | HTTP Method | Retry Policy | Circuit Breaker |
|-----------|-------------|--------------|-----------------|
| GET | GET | Full (3 retries, exponential backoff) | Yes |
| POST | POST | Limited (timeout only) | No |
| PUT | PUT | Full (3 retries, exponential backoff) | Yes |
| DELETE | DELETE | Full (3 retries, exponential backoff) | Yes |

## Troubleshooting

### No Telemetry Logs Appearing

**Issue:** `[Resilience]` logs not visible in Debug Output.

**Solution:**
- Ensure you're running in Debug mode (not Release)
- Check that logging is configured in `MauiProgram.cs`
- Verify `Debug` logging is enabled (enabled by default in DEBUG builds)

### Circuit Breaker Not Opening

**Issue:** Circuit breaker doesn't open after multiple failures.

**Solution:**
- Circuit breaker opens after 5 consecutive failures by default
- Ensure you're sending enough requests (at least 5)
- Check that failures are transient errors (5xx, 408, 429)
- Wait for requests to complete (circuit state changes are synchronous)

### Retries Not Happening

**Issue:** Requests fail immediately without retrying.

**Solution:**
- Ensure you're using a named HttpClient configured with `AddStandardResiliencePolicy()`
- Check that the error is a transient error (408, 429, 5xx except 501, 505)
- Verify the HttpClient is created via `IHttpClientFactory` (not `new HttpClient()`)
- For POST requests, retries only happen on timeout/connection errors

## Next Steps

- Integrate resilience policies with your own API services
- Customize retry counts and delays for specific endpoints
- Add custom policies for specific use cases
- Monitor telemetry in production (Application Insights, etc.)

## See Also

- [Resilience Policies Documentation](resilience-policies.md) - Full API documentation
- [Polly Documentation](https://www.pollydocs.org/) - Polly library documentation
- [httpbin.org](https://httpbin.org) - HTTP testing service used for examples

