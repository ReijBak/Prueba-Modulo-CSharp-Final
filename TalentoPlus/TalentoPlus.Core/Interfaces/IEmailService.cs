namespace TalentoPlus.Core.Interfaces;

/// <summary>
/// Interface for email service operations.
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendWelcomeEmailAsync(string to, string employeeName);
    Task SendWelcomeEmailWithCredentialsAsync(string to, string employeeName, long documento, string password);
}

