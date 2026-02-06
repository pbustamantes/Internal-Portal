using MediatR;

namespace InternalPortal.Application.Features.Auth.Commands;

public record RevokeTokenCommand(string RefreshToken) : IRequest<Unit>;
