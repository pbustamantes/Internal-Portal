using InternalPortal.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace InternalPortal.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        // Stub implementation - log instead of sending
        _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
        return Task.CompletedTask;
    }
}
