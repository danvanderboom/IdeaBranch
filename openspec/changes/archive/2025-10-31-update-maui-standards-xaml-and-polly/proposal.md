## Why
Align our .NET MAUI apps on consistent UI and resilience standards: XAML-first UI authoring with C# code, and Polly for robust retry/backoff behavior.

## What Changes
- Standardize on XAML for UI definitions with C# code-behind and/or MVVM viewmodels.
- Require the latest stable C# language version supported by the target .NET SDK (`LangVersion`=latest).
- Adopt Polly for retry policies, using exponential backoff with jitter, plus circuit breakers where applicable.
- Centralize policy registration and integrate with `HttpClientFactory` and outbound I/O.

## Impact
- Affected specs: ui, platforms, error-handling, performance
- Affected code: MAUI project configuration (e.g., `Directory.Build.props`), DI registrations for Polly policies and `HttpClientFactory` handlers, shared guidance/examples.

