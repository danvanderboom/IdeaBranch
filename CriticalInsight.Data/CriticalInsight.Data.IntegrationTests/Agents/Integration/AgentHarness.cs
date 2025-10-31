using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CriticalInsight.Data.IntegrationTests.Agents.Integration;

public static class AgentHarness
{
    private static readonly Regex ChoiceRegex = new("[ABCD]", RegexOptions.Compiled);

    public sealed class RunOptions
    {
        public int Trials { get; set; } = GetEnvInt("CI_AF_TRIALS", 20);
        public double TargetSuccessRate { get; set; } = GetDefaultTargetSuccessRate();
        public double DeltaMin { get; set; } = GetDefaultDelta();
        // Temperature control depends on provider; left for future wiring
        public double Temperature { get; set; } = GetEnvDouble("CI_AF_TEMP", 0.2);
        public int PerCallTimeoutMs { get; set; } = GetEnvInt("CI_AF_PER_CALL_TIMEOUT_MS", 5000);
    }

    public sealed class RunResult
    {
        public int Trials { get; init; }
        public int NumCorrect { get; init; }
        public double SuccessRate => Trials == 0 ? 0 : (double)NumCorrect / Trials;
    }

    public static async Task<RunResult> RunTrialsAsync(IAgentRunner agent, Func<int, string> promptFactory, Func<char> correctAnswer, RunOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new RunOptions();
        int correct = 0;
        for (int i = 0; i < options.Trials; i++)
        {
            string prompt = promptFactory(i);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            if (options.PerCallTimeoutMs > 0)
            {
                cts.CancelAfter(options.PerCallTimeoutMs);
            }

            string output = await agent.RunAsync(prompt, cts.Token).ConfigureAwait(false);
            char? choice = ParseChoice(output);
            if (choice.HasValue && choice.Value == correctAnswer())
            {
                correct++;
            }
        }

        return new RunResult { Trials = options.Trials, NumCorrect = correct };
    }

    public static char? ParseChoice(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var m = ChoiceRegex.Match(text.ToUpperInvariant());
        return m.Success ? m.Value[0] : (char?)null;
    }

    private static int GetEnvInt(string name, int fallback)
        => int.TryParse(Environment.GetEnvironmentVariable(name), out var v) ? v : fallback;

    private static double GetEnvDouble(string name, double fallback)
        => double.TryParse(Environment.GetEnvironmentVariable(name), out var v) ? v : fallback;

    private static double GetDefaultTargetSuccessRate()
    {
        var env = Environment.GetEnvironmentVariable("CI_AF_TARGET_SUCCESS");
        if (double.TryParse(env, out var fromEnv)) return fromEnv;
        var provider = AgentProvider.GetProvider();
        return provider == "lmstudio" ? 0.6 : 0.9;
    }

    private static double GetDefaultDelta()
    {
        var env = Environment.GetEnvironmentVariable("CI_AF_DELTA");
        if (double.TryParse(env, out var fromEnv)) return fromEnv;
        var provider = AgentProvider.GetProvider();
        return provider == "lmstudio" ? 0.2 : 0.3;
    }
}


