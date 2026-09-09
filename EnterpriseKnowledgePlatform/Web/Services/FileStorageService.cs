using Microsoft.AspNetCore.Http;

namespace Web.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public FileStorageService(IConfiguration configuration, IHostEnvironment environment)
    {
        var configuredPath = configuration["FileStorage:BasePath"] ?? "App_Data/uploads";
        _basePath = Path.GetFullPath(Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(environment.ContentRootPath, configuredPath));
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveAsync(
        IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length <= 0)
        {
            throw new ArgumentException("Uploaded file must not be empty.");
        }

        var extension = Path.GetExtension(file.FileName);
        var storedName = $"{Guid.NewGuid()}{extension}";
        var fullPath = GetSafePath(storedName);
        Directory.CreateDirectory(_basePath);

        await using var stream = new FileStream(
            fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(stream, cancellationToken);

        return storedName;
    }

    public Task<Stream> OpenReadAsync(
        string fileReference, CancellationToken cancellationToken = default)
    {
        var fullPath = GetSafePath(fileReference);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Stored document file was not found.");
        }

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(
        string fileReference, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileReference))
        {
            return Task.CompletedTask;
        }

        var fullPath = GetSafePath(fileReference);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string GetSafePath(string fileReference)
    {
        var fileName = Path.GetFileName(fileReference);
        if (!string.Equals(fileName, fileReference, StringComparison.Ordinal) ||
            fileName.Contains(Path.DirectorySeparatorChar) ||
            fileName.Contains(Path.AltDirectorySeparatorChar))
        {
            throw new ArgumentException("Invalid stored file reference.");
        }

        return Path.Combine(_basePath, fileName);
    }
}
