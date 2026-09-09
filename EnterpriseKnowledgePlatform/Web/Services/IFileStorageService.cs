namespace Web.Services;

public interface IFileStorageService
{
    Task<string> SaveAsync(IFormFile file, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string fileReference, CancellationToken cancellationToken = default);
    Task DeleteAsync(string fileReference, CancellationToken cancellationToken = default);
}
