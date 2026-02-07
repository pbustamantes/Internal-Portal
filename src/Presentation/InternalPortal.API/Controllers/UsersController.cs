using InternalPortal.Application.Common.Exceptions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Application.Features.Auth.DTOs;
using InternalPortal.Application.Features.Registrations.Queries;
using InternalPortal.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace InternalPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _env;

    public UsersController(IMediator mediator, ICurrentUserService currentUserService, IUserRepository userRepository, IUnitOfWork unitOfWork, IWebHostEnvironment env)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _env = env;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenException();
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        return Ok(new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.Department, user.Role.ToString(), user.ProfilePictureUrl));
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

        return Ok(new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.Department, user.Role.ToString(), user.ProfilePictureUrl));
    }

    [HttpPost("me/profile-picture")]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        if (file.Length == 0)
            return BadRequest(new { detail = "File is empty." });

        if (file.Length > MaxFileSize)
            return BadRequest(new { detail = "File size exceeds 5 MB limit." });

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
            return BadRequest(new { detail = "Only jpg, png, gif, and webp files are allowed." });

        var userId = _currentUserService.UserId ?? throw new ForbiddenException();
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        var uploadsDir = Path.Combine(_env.ContentRootPath, "uploads", "profile-pictures");
        Directory.CreateDirectory(uploadsDir);

        // Remove old picture if it exists
        DeleteProfilePictureFile(user);

        var fileName = $"{userId}{extension}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        user.ProfilePictureUrl = $"/uploads/profile-pictures/{fileName}";
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.Department, user.Role.ToString(), user.ProfilePictureUrl));
    }

    [HttpDelete("me/profile-picture")]
    public async Task<IActionResult> DeleteProfilePicture()
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenException();
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        DeleteProfilePictureFile(user);

        user.ProfilePictureUrl = null;
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.Department, user.Role.ToString(), user.ProfilePictureUrl));
    }

    private void DeleteProfilePictureFile(Domain.Entities.User user)
    {
        if (string.IsNullOrEmpty(user.ProfilePictureUrl)) return;

        var existingPath = Path.Combine(_env.ContentRootPath, user.ProfilePictureUrl.TrimStart('/'));
        if (System.IO.File.Exists(existingPath))
            System.IO.File.Delete(existingPath);
    }

    [HttpGet("me/events")]
    public async Task<IActionResult> GetMyRegistrations()
    {
        var result = await _mediator.Send(new GetUserRegistrationsQuery());
        return Ok(result);
    }
}

public record UpdateProfileRequest(string FirstName, string LastName, string? Department);
