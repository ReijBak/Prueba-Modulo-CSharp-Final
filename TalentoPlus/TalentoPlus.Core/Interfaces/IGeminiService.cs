namespace TalentoPlus.Core.Interfaces;

/// <summary>
/// Interface for Gemini AI service operations.
/// </summary>
public interface IGeminiService
{
    /// <summary>
    /// Converts a natural language question into SQL and executes it.
    /// </summary>
    /// <param name="question">Natural language question</param>
    /// <returns>Query result as JSON string</returns>
    Task<string> QueryDatabaseAsync(string question);
}

