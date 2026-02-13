using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Application.Common.Security;
using InternalPortal.Application.Features.Auth.DTOs;
using InternalPortal.Domain.Entities;
using InternalPortal.Domain.Interfaces;
using MediatR;

namespace InternalPortal.Application.Features.Auth.Commands;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IJwtService jwtService,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var requestTokenHash = TokenHasher.HashToken(request.RefreshToken);
        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(requestTokenHash, cancellationToken)
            ?? throw new ApplicationException("Invalid refresh token.");

        if (!existingToken.IsActive)
            throw new ApplicationException("Token is expired or revoked.");

        var user = await _userRepository.GetByIdAsync(existingToken.UserId, cancellationToken)
            ?? throw new ApplicationException("User not found.");

        // Rotate token
        var newRawToken = _jwtService.GenerateRefreshToken();
        existingToken.RevokedAtUtc = DateTime.UtcNow;
        existingToken.ReplacedByTokenHash = TokenHasher.HashToken(newRawToken);
        await _refreshTokenRepository.UpdateAsync(existingToken, cancellationToken);

        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = TokenHasher.HashToken(newRawToken),
            UserId = user.Id,
            ExpiresUtc = DateTime.UtcNow.AddDays(7),
            CreatedAtUtc = DateTime.UtcNow
        };

        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

        var accessToken = _jwtService.GenerateAccessToken(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            accessToken,
            newRawToken,
            DateTime.UtcNow.AddHours(1),
            new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.Department, user.Role.ToString(), user.ProfilePictureUrl));
    }
}
