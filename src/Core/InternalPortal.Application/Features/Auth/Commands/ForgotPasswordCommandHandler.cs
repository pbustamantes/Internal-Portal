using System.Reflection;
using System.Security.Cryptography;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace InternalPortal.Application.Features.Auth.Commands;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null || !user.IsActive)
            return Unit.Value;

        var rawToken = GenerateToken();
        var hashedToken = HashToken(rawToken);

        user.PasswordResetToken = hashedToken;
        user.PasswordResetTokenExpiresUtc = DateTime.UtcNow.AddHours(1);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
        var resetLink = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(rawToken)}&email={Uri.EscapeDataString(request.Email)}";

        var htmlBody = await LoadTemplateAsync("ForgotPassword.html", cancellationToken);
        htmlBody = htmlBody.Replace("{{ResetLink}}", resetLink);

        await _emailService.SendEmailAsync(
            request.Email,
            "Reset Your Password",
            htmlBody,
            cancellationToken);

        return Unit.Value;
    }

    private static string GenerateToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private static string HashToken(string token)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    private static async Task<string> LoadTemplateAsync(string templateName, CancellationToken cancellationToken)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"InternalPortal.Application.Templates.{templateName}";
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Email template '{templateName}' not found.");
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}
