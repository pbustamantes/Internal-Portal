using InternalPortal.Application.Features.Auth.DTOs;
using MediatR;

namespace InternalPortal.Application.Features.Auth.Commands;

public record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Department) : IRequest<AuthResponse>;
