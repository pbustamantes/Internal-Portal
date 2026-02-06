using InternalPortal.Application.Common.Exceptions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Application.Features.Auth.DTOs;
using InternalPortal.Application.Features.Registrations.Queries;
using InternalPortal.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternalPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UsersController(IMediator mediator, ICurrentUserService currentUserService, IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenException();
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        return Ok(new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.Department, user.Role.ToString()));
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenException();
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Department = request.Department;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.Department, user.Role.ToString()));
    }

    [HttpGet("me/events")]
    public async Task<IActionResult> GetMyRegistrations()
    {
        var result = await _mediator.Send(new GetUserRegistrationsQuery());
        return Ok(result);
    }
}

public record UpdateProfileRequest(string FirstName, string LastName, string? Department);
