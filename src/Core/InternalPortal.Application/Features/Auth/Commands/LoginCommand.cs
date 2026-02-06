using InternalPortal.Application.Features.Auth.DTOs;
using MediatR;

namespace InternalPortal.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;
