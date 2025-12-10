using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using TalentoPlus.Core.Interfaces;

namespace TalentoPlus.Infrastructure.Services;

/// <summary>
/// Email service implementation using MailKit for SMTP.
/// </summary>
public class EmailService : IEmailService
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
        _smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "smtp.gmail.com";
        _smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
        _smtpUsername = Environment.GetEnvironmentVariable("SMTP_USERNAME") ?? "";
        _smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? "";
        _fromEmail = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL") ?? _smtpUsername;
        _fromName = Environment.GetEnvironmentVariable("SMTP_FROM_NAME") ?? "TalentoPlus";

        _logger.LogInformation("EmailService initialized - Host: {Host}, Port: {Port}, Username: {Username}, FromEmail: {FromEmail}", 
            _smtpHost, _smtpPort, _smtpUsername, _fromEmail);
        
        if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
        {
            _logger.LogWarning("SMTP credentials are not configured properly!");
        }
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogInformation("Preparing to send email to {To} with subject: {Subject}", to, subject);
        
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _fromEmail));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            _logger.LogInformation("Connecting to SMTP {Host}:{Port}...", _smtpHost, _smtpPort);
            
            // Use STARTTLS for port 587, SSL for port 465
            var secureSocketOptions = _smtpPort == 465 
                ? SecureSocketOptions.SslOnConnect 
                : SecureSocketOptions.StartTls;
            
            await client.ConnectAsync(_smtpHost, _smtpPort, secureSocketOptions);
            
            _logger.LogInformation("Authenticating with SMTP server...");
            await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
            
            _logger.LogInformation("Sending email...");
            await client.SendAsync(message);
            
            await client.DisconnectAsync(true);
            
            _logger.LogInformation("Email sent successfully to {Email}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}. Error: {Error}", to, ex.Message);
            throw;
        }
    }

    public async Task SendWelcomeEmailAsync(string to, string employeeName)
    {
        var subject = "¡Bienvenido a TalentoPlus!";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>¡Hola {employeeName}!</h2>
                <p>Te damos la bienvenida a <strong>TalentoPlus</strong>.</p>
                <p>Tu cuenta ha sido creada exitosamente en nuestro sistema de gestión de empleados.</p>
                <p>Si tienes alguna pregunta, no dudes en contactar al departamento de Recursos Humanos.</p>
                <br/>
                <p>Saludos cordiales,</p>
                <p><strong>Equipo TalentoPlus</strong></p>
            </body>
            </html>";

        await SendEmailAsync(to, subject, body);
    }

    public async Task SendWelcomeEmailWithCredentialsAsync(string to, string employeeName, long documento, string password)
    {
        var subject = "¡Bienvenido a TalentoPlus! - Tus credenciales de acceso";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background-color: #2563eb; color: white; padding: 20px; text-align: center;'>
                    <h1 style='margin: 0;'>TalentoPlus</h1>
                </div>
                <div style='padding: 30px; background-color: #f9fafb;'>
                    <h2 style='color: #1f2937;'>¡Hola {employeeName}!</h2>
                    <p style='color: #4b5563;'>Te damos la bienvenida a <strong>TalentoPlus</strong>.</p>
                    <p style='color: #4b5563;'>Tu cuenta ha sido creada exitosamente. A continuación encontrarás tus credenciales de acceso:</p>
                    
                    <div style='background-color: white; border: 1px solid #e5e7eb; border-radius: 8px; padding: 20px; margin: 20px 0;'>
                        <h3 style='color: #1f2937; margin-top: 0;'>🔐 Credenciales de Acceso</h3>
                        <p style='margin: 10px 0;'><strong>Usuario (Documento):</strong> <code style='background-color: #f3f4f6; padding: 4px 8px; border-radius: 4px;'>{documento}</code></p>
                        <p style='margin: 10px 0;'><strong>Contraseña:</strong> <code style='background-color: #f3f4f6; padding: 4px 8px; border-radius: 4px;'>{password}</code></p>
                    </div>
                    
                    <p style='color: #dc2626; font-size: 14px;'>⚠️ <strong>Importante:</strong> Te recomendamos cambiar tu contraseña después del primer inicio de sesión.</p>
                    
                    <p style='color: #4b5563;'>Si tienes alguna pregunta, no dudes en contactar al departamento de Recursos Humanos.</p>
                    
                    <br/>
                    <p style='color: #6b7280;'>Saludos cordiales,</p>
                    <p style='color: #1f2937;'><strong>Equipo TalentoPlus</strong></p>
                </div>
                <div style='background-color: #1f2937; color: #9ca3af; padding: 15px; text-align: center; font-size: 12px;'>
                    <p style='margin: 0;'>Este es un correo automático, por favor no responder.</p>
                </div>
            </body>
            </html>";

        await SendEmailAsync(to, subject, body);
    }
}

