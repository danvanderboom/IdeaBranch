using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IdeaBranch.App.Services.LLM;

/// <summary>
/// Interface for LLM client implementations.
/// Supports both local (LM Studio) and cloud (Azure OpenAI) providers.
/// </summary>
public interface ILLMClient
{
    /// <summary>
    /// Generates a response for the given prompt, using context from parent nodes.
    /// </summary>
    /// <param name="prompt">The user's prompt/question.</param>
    /// <param name="contextPath">Context from parent nodes (root to parent), used to build conversation context.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The generated response text.</returns>
    Task<string> GenerateResponseAsync(string prompt, IEnumerable<string> contextPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Suggests a title for the given prompt and response pair.
    /// </summary>
    /// <param name="prompt">The user's prompt/question.</param>
    /// <param name="response">The generated response.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A suggested title, or null if generation fails.</returns>
    Task<string?> SuggestTitleAsync(string prompt, string response, CancellationToken cancellationToken = default);
}

