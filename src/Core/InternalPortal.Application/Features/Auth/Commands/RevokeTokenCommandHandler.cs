using InternalPortal.Application.Common.Exceptions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Application.Common.Security;
using InternalPortal.Domain.Interfaces;
using MediatR;

namespace InternalPortal.Application.Features.Auth.Commands;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Unit>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RevokeTokenCommandHandler(IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = TokenHasher.HashToken(request.RefreshToken);
        var token = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken)
            ?? throw new ApplicationException("Invalid refresh token.");

        if (token.UserId != _currentUserService.UserId)
            throw new ForbiddenException("You can only revoke your own refresh tokens.");

        if (!token.IsActive)
            throw new ApplicationException("Token is already revoked.");

        token.RevokedAtUtc = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(token, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
