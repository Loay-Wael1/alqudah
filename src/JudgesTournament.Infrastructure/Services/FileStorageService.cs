using JudgesTournament.Application.Interfaces;
using JudgesTournament.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JudgesTournament.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly UploadOptions _options;
    private readonly ILogger<FileStorageService> _logger;
    private readonly string _basePath;

    public FileStorageService(IOptions<UploadOptions> options, ILogger<FileStorageService> logger, IWebHostEnvironmentAccessor hostAccessor)
    {
        _options = options.Value;
        _logger = logger;
        _basePath = Path.Combine(hostAccessor.ContentRootPath, _options.ReceiptsPath);
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveReceiptAsync(Stream fileStream, string originalFileName, CancellationToken cancellationToken = default)
    {
        // Level 2 validation (service level)
        if (fileStream is null || fileStream.Length == 0)
            throw new InvalidOperationException("الملف فارغ أو غير صالح.");

        if (fileStream.Length > _options.MaxReceiptSizeBytes)
            throw new InvalidOperationException($"حجم الملف يتجاوز الحد المسموح ({_options.MaxReceiptSizeMb} ميجابايت).");

        var extension = Path.GetExtension(originalFileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !_options.AllowedExtensions.Contains(extension))
            throw new InvalidOperationException("نوع الملف غير مسموح. الأنواع المسموحة: " + string.Join(", ", _options.AllowedExtensions));

        // Safe file naming with GUID
        var safeFileName = $"{Guid.NewGuid()}{extension}";
        var relativePath = Path.Combine(_options.ReceiptsPath, safeFileName);
        var fullPath = Path.Combine(_basePath, safeFileName);

        await using var fileStreamOut = new FileStream(fullPath, FileMode.Create);
        await fileStream.CopyToAsync(fileStreamOut, cancellationToken);

        _logger.LogInformation("Receipt saved: {Path}", relativePath);
        return relativePath;
    }

    public void DeleteReceipt(string relativePath)
    {
        var fullPath = GetFullPath(relativePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("Receipt deleted: {Path}", relativePath);
        }
    }

    public string GetFullPath(string relativePath)
    {
        // Extract just the filename to prevent path traversal
        var fileName = Path.GetFileName(relativePath);
        return Path.Combine(_basePath, fileName);
    }
}

/// <summary>
/// Simple accessor to provide ContentRootPath to FileStorageService without depending on IWebHostEnvironment directly in Infrastructure.
/// </summary>
public interface IWebHostEnvironmentAccessor
{
    string ContentRootPath { get; }
}
