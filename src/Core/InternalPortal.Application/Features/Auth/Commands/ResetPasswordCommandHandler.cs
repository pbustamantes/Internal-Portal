using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Application.Common.Security;
using InternalPortal.Domain.Entities;
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

        if (!HasValidResetToken(user, request.Token))
            throw new ApplicationException("Invalid or expired reset token.");

        user!.PasswordHash = _identityService.HashPassword(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiresUtc = null;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private static bool HasValidResetToken(User? user, string token)
    {
        if (user is null || string.IsNullOrEmpty(user.PasswordResetToken))
            return false;

        var hashedToken = TokenHasher.HashToken(token);

        return user.PasswordResetToken == hashedToken
            && user.PasswordResetTokenExpiresUtc is not null
            && user.PasswordResetTokenExpiresUtc > DateTime.UtcNow;
    }
}
