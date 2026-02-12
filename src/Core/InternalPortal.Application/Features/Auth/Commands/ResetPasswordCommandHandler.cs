using System.Security.Cryptography;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Domain.Interfaces;
using MediatR;

namespace InternalPortal.Application.Features.Auth.Commands;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IIdentityService identityService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _identityService = identityService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
            throw new ApplicationException("Invalid or expired reset token.");

        if (string.IsNullOrEmpty(user.PasswordResetToken))
            throw new ApplicationException("Invalid or expired reset token.");

        var hashedToken = HashToken(request.Token);

        if (user.PasswordResetToken != hashedToken)
            throw new ApplicationException("Invalid or expired reset token.");

        if (user.PasswordResetTokenExpiresUtc is null || user.PasswordResetTokenExpiresUtc < DateTime.UtcNow)
            throw new ApplicationException("Invalid or expired reset token.");

        user.PasswordHash = _identityService.HashPassword(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiresUtc = null;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private static string HashToken(string token)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
