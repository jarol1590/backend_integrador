using MailKit.Net.Smtp;
using MimeKit;

public class EmailService : IEmailService
{
    public async Task SendAsync(
        string to,
        string subject,
        string htmlBody)
    {
        var email = new MimeMessage();

        email.From.Add(
            MailboxAddress.Parse("tucorreo@gmail.com"));

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
            "smtp.gmail.com",
            587,
            MailKit.Security.SecureSocketOptions.StartTls);

        await smtp.AuthenticateAsync(
            "tucorreo@gmail.com",
            "APP_PASSWORD");

        await smtp.SendAsync(email);

        await smtp.DisconnectAsync(true);
    }
}