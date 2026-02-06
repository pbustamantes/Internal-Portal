using InternalPortal.Application.Features.Auth.DTOs;
using MediatR;

namespace InternalPortal.Application.Features.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;
