using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Service.Interface;

namespace Service.Implementation;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        var host = Required("Email:SmtpHost");
        var username = Required("Email:Username");
        var password = Required("Email:Password");
        var fromAddress = Required("Email:FromAddress");
        var fromName = _configuration["Email:FromName"] ?? fromAddress;
        if (!int.TryParse(_configuration["Email:SmtpPort"], out var port) || port <= 0)
        {
            throw new InvalidOperationException("Email SMTP port configuration is invalid.");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(fromAddress, fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        message.To.Add(new MailAddress(to));

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(username, password)
        };
        await client.SendMailAsync(message, cancellationToken);
    }

    private string Required(string key) =>
        !string.IsNullOrWhiteSpace(_configuration[key])
            ? _configuration[key]!
            : throw new InvalidOperationException($"Email configuration '{key}' is required.");
}
