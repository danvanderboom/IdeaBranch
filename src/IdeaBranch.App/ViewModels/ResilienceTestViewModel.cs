using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using IdeaBranch.App.Services;
using Microsoft.Maui.Controls;

namespace IdeaBranch.App.ViewModels;

/// <summary>
/// ViewModel for ResilienceTestPage that demonstrates resilience policies.
/// </summary>
public sealed class ResilienceTestViewModel : INotifyPropertyChanged
{
    private readonly ExampleApiService _apiService;
    private string _statusMessage = "Ready";
    private Color _statusColor = Colors.Black;
    private bool _isBusy;
    private string _results = string.Empty;

    public ResilienceTestViewModel(ExampleApiService apiService)
    {
        _apiService = apiService;
        
        TestGetCommand = new Command(async () => await TestGetAsync(), () => !IsBusy);
        TestPostCommand = new Command(async () => await TestPostAsync(), () => !IsBusy);
        TestRetryCommand = new Command(async () => await TestRetryAsync(), () => !IsBusy);
        TestCircuitBreakerCommand = new Command(async () => await TestCircuitBreakerAsync(), () => !IsBusy);
        TestDelayCommand = new Command(async () => await TestDelayAsync(), () => !IsBusy);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }
    }

    public Color StatusColor
    {
        get => _statusColor;
        private set
        {
            if (_statusColor != value)
            {
                _statusColor = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (_isBusy != value)
            {
                _isBusy = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotBusy));
                
                // Update command enabled states
                (TestGetCommand as Command)?.ChangeCanExecute();
                (TestPostCommand as Command)?.ChangeCanExecute();
                (TestRetryCommand as Command)?.ChangeCanExecute();
                (TestCircuitBreakerCommand as Command)?.ChangeCanExecute();
                (TestDelayCommand as Command)?.ChangeCanExecute();
            }
        }
    }

    public bool IsNotBusy => !IsBusy;

    public string Results
    {
        get => _results;
        private set
        {
            if (_results != value)
            {
                _results = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand TestGetCommand { get; }
    public ICommand TestPostCommand { get; }
    public ICommand TestRetryCommand { get; }
    public ICommand TestCircuitBreakerCommand { get; }
    public ICommand TestDelayCommand { get; }

    private async Task TestGetAsync()
    {
        IsBusy = true;
        StatusMessage = "Testing GET (idempotent - full retry)...";
        StatusColor = Colors.Blue;
        AppendResult("=== Testing GET Request ===\n");

        try
        {
            var result = await _apiService.GetTestDataAsync();
            
            StatusMessage = "GET succeeded!";
            StatusColor = Colors.Green;
            AppendResult($"Success! URL: {result?.Url}\nOrigin: {result?.Origin}\n\n");
        }
        catch (Exception ex)
        {
            StatusMessage = $"GET failed: {ex.Message}";
            StatusColor = Colors.Red;
            AppendResult($"Failed: {ex.Message}\n\n");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task TestPostAsync()
    {
        IsBusy = true;
        StatusMessage = "Testing POST (non-idempotent - limited retry)...";
        StatusColor = Colors.Blue;
        AppendResult("=== Testing POST Request ===\n");

        try
        {
            var testData = new { message = "Test data", timestamp = DateTime.UtcNow };
            var result = await _apiService.PostTestDataAsync(testData);
            
            StatusMessage = "POST succeeded!";
            StatusColor = Colors.Green;
            AppendResult($"Success! Posted data echoed back.\n\n");
        }
        catch (Exception ex)
        {
            StatusMessage = $"POST failed: {ex.Message}";
            StatusColor = Colors.Red;
            AppendResult($"Failed: {ex.Message}\n\n");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task TestRetryAsync()
    {
        IsBusy = true;
        StatusMessage = "Testing retry with 500 error...";
        StatusColor = Colors.Blue;
        AppendResult("=== Testing Retry Behavior (500 Error) ===\n");
        AppendResult("Note: 500 errors trigger retries with exponential backoff.\n");
        AppendResult("Check Debug Output for [Resilience] retry logs.\n\n");

        try
        {
            var success = await _apiService.TestRetryBehaviorAsync(500);
            
            if (success)
            {
                StatusMessage = "Retry test completed";
                StatusColor = Colors.Orange;
                AppendResult("Request completed (after retries).\n\n");
            }
            else
            {
                StatusMessage = "Retry test: Request failed";
                StatusColor = Colors.Orange;
                AppendResult("Request failed after retries (expected for 500 error).\n");
                AppendResult("Check logs for retry attempts with exponential backoff.\n\n");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Retry test error: {ex.Message}";
            StatusColor = Colors.Red;
            AppendResult($"Error: {ex.Message}\n\n");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task TestCircuitBreakerAsync()
    {
        IsBusy = true;
        StatusMessage = "Testing circuit breaker (triggering failures)...";
        StatusColor = Colors.Blue;
        AppendResult("=== Testing Circuit Breaker ===\n");
        AppendResult("Sending multiple requests that will fail...\n");
        AppendResult("After enough failures, circuit should open.\n");
        AppendResult("Check Debug Output for [Resilience] circuit breaker logs.\n\n");

        try
        {
            var circuitBrokenCount = await _apiService.TestCircuitBreakerAsync(10);
            
            StatusMessage = $"Circuit breaker test complete ({circuitBrokenCount} circuit breaks)";
            StatusColor = circuitBrokenCount > 0 ? Colors.Orange : Colors.Green;
            AppendResult($"Circuit breaker test complete.\n");
            AppendResult($"Circuit broken/rejected requests: {circuitBrokenCount}\n");
            AppendResult($"This means circuit opened after consecutive failures.\n\n");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Circuit breaker test error: {ex.Message}";
            StatusColor = Colors.Red;
            AppendResult($"Error: {ex.Message}\n\n");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task TestDelayAsync()
    {
        IsBusy = true;
        StatusMessage = "Testing delay/timeout behavior...";
        StatusColor = Colors.Blue;
        AppendResult("=== Testing Delay/Timeout ===\n");
        AppendResult("Requesting a delayed response to test timeout handling.\n\n");

        try
        {
            // Request a 5-second delay, but our timeout is shorter
            var success = await _apiService.TestDelayAsync(5);
            
            if (success)
            {
                StatusMessage = "Delay test: Request succeeded";
                StatusColor = Colors.Green;
                AppendResult("Request completed (within timeout).\n\n");
            }
            else
            {
                StatusMessage = "Delay test: Request timed out";
                StatusColor = Colors.Orange;
                AppendResult("Request timed out (expected).\n");
                AppendResult("Retries may have occurred for timeout errors.\n\n");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Delay test error: {ex.Message}";
            StatusColor = Colors.Red;
            AppendResult($"Error: {ex.Message}\n\n");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void AppendResult(string text)
    {
        var builder = new StringBuilder(Results);
        builder.Append($"[{DateTime.Now:HH:mm:ss}] {text}");
        Results = builder.ToString();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

