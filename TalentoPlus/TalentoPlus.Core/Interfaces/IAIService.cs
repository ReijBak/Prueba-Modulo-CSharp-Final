namespace TalentoPlus.Core.Interfaces;

/// <summary>
/// Interface for AI services that convert natural language to SQL.
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Converts a natural language question to SQL and executes it.
    /// </summary>
    /// <param name="question">The question in natural language.</param>
    /// <returns>JSON string with the query results.</returns>
    Task<string> QueryDatabaseAsync(string question);
}

