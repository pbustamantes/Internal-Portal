using InternalPortal.Application.Features.Auth.DTOs;
using MediatR;

namespace InternalPortal.Application.Features.Users.Commands;

public record DeleteProfilePictureCommand : IRequest<UserDto>;
