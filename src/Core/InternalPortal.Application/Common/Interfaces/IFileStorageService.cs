namespace InternalPortal.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveProfilePictureAsync(Guid userId, string extension, Stream fileStream, CancellationToken cancellationToken = default);
    void DeleteProfilePicture(string relativeUrl);
}
