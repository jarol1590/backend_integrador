using System.Diagnostics;
using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Common;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace BackendIntegrador.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(EmailSettings emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        var maskedTo = MaskEmail(to);
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Iniciando envío SMTP. To={MaskedTo}, Subject={Subject}, Server={SmtpServer}, Port={Port}, Sender={SenderEmail}, UsernameConfigured={UsernameConfigured}, PasswordConfigured={PasswordConfigured}",
            maskedTo,
            subject,
            _emailSettings.SmtpServer,
            _emailSettings.Port,
            _emailSettings.SenderEmail,
            !string.IsNullOrWhiteSpace(_emailSettings.Username),
            !string.IsNullOrWhiteSpace(_emailSettings.Password));

        if (string.IsNullOrWhiteSpace(_emailSettings.SmtpServer))
        {
            _logger.LogError("EmailSettings.SmtpServer no está configurado.");
            throw new InvalidOperationException("EmailSettings.SmtpServer no está configurado.");
        }

        if (_emailSettings.Port <= 0)
        {
            _logger.LogError("EmailSettings.Port no es válido: {Port}", _emailSettings.Port);
            throw new InvalidOperationException("EmailSettings.Port no está configurado correctamente.");
        }

        var email = new MimeMessage();

        email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        email.Body = new TextPart("html") { Text = htmlBody };

        using var smtp = new SmtpClient();

        try
        {
            _logger.LogDebug("SMTP paso ConnectAsync iniciado.");
            await smtp.ConnectAsync(
                _emailSettings.SmtpServer,
                _emailSettings.Port,
                MailKit.Security.SecureSocketOptions.StartTls);
            _logger.LogInformation(
                "SMTP ConnectAsync completado en {ElapsedMs}ms. Server={SmtpServer}, Port={Port}",
                stopwatch.ElapsedMilliseconds,
                _emailSettings.SmtpServer,
                _emailSettings.Port);

            _logger.LogDebug("SMTP paso AuthenticateAsync iniciado. Username={Username}", _emailSettings.Username);
            await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            _logger.LogInformation(
                "SMTP AuthenticateAsync completado en {ElapsedMs}ms. Username={Username}",
                stopwatch.ElapsedMilliseconds,
                _emailSettings.Username);

            _logger.LogDebug("SMTP paso SendAsync iniciado.");
            await smtp.SendAsync(email);
            _logger.LogInformation(
                "SMTP SendAsync completado en {ElapsedMs}ms. To={MaskedTo}, Subject={Subject}",
                stopwatch.ElapsedMilliseconds,
                maskedTo,
                subject);

            await smtp.DisconnectAsync(true);
            _logger.LogInformation(
                "Envío SMTP finalizado correctamente en {ElapsedMs}ms. To={MaskedTo}",
                stopwatch.ElapsedMilliseconds,
                maskedTo);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error SMTP tras {ElapsedMs}ms. To={MaskedTo}, Subject={Subject}, Server={SmtpServer}, Port={Port}, ExceptionType={ExceptionType}",
                stopwatch.ElapsedMilliseconds,
                maskedTo,
                subject,
                _emailSettings.SmtpServer,
                _emailSettings.Port,
                ex.GetType().Name);

            throw new InvalidOperationException(
                "Error al enviar el email. Por favor, revise la configuración del servidor SMTP.",
                ex);
        }
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return "(vacío)";

        var at = email.IndexOf('@');
        if (at <= 0)
            return "***";

        return email[0] + "***" + email[at..];
    }
}
