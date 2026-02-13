using InternalPortal.Application.Common.Interfaces;

namespace InternalPortal.API.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;

    public FileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveProfilePictureAsync(Guid userId, string extension, Stream fileStream, CancellationToken cancellationToken = default)
    {
        var uploadsDir = Path.Combine(_env.ContentRootPath, "uploads", "profile-pictures");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{userId}{extension}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using var output = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(output, cancellationToken);

        return $"/uploads/profile-pictures/{fileName}";
    }

    public void DeleteProfilePicture(string relativeUrl)
    {
        var filePath = Path.Combine(_env.ContentRootPath, relativeUrl.TrimStart('/'));
        if (File.Exists(filePath))
            File.Delete(filePath);
    }
}
