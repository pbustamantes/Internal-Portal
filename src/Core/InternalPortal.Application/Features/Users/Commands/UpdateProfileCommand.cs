using InternalPortal.Application.Features.Auth.DTOs;
using MediatR;

namespace InternalPortal.Application.Features.Users.Commands;

public record UpdateProfileCommand(string FirstName, string LastName, string? Department) : IRequest<UserDto>;
