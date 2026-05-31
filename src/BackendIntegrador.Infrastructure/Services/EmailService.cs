using MailKit.Net.Smtp;
using MimeKit;
using BackendIntegrador.Application.Common;
using BackendIntegrador.Application.Abstractions;

namespace BackendIntegrador.Infrastructure.Services;

public class EmailService : IEmailService
{
    public async Task SendAsync(
        string to,
        string subject,
        string htmlBody)
    {
        var email = new MimeMessage();

        email.From.Add(
           new MailboxAddress(_emailSettings.SenderName,_emailSettings.SenderEmail));

        email.To.Add(
            MailboxAddress.Parse(to));

        email.Subject = subject;

        email.Body =
            new TextPart("html")
            {
                Text = htmlBody
            };

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(
            _emailSettings.SmtpServer,
            _emailSettings.Port,
            MailKit.Security.SecureSocketOptions.StartTls);

        await smtp.AuthenticateAsync(
            _emailSettings.Username,
            _emailSettings.Password);

        await smtp.SendAsync(email);

        await smtp.DisconnectAsync(true);
    }

    private readonly EmailSettings _emailSettings;

    public EmailService(EmailSettings emailSettings)
    {
        _emailSettings = emailSettings;
    }
}