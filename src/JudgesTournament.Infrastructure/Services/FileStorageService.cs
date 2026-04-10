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
        {
            _logger.LogWarning("Empty or null file upload attempted: {FileName}", Path.GetFileName(originalFileName));
            throw new InvalidOperationException("الملف فارغ أو غير صالح.");
        }

        if (fileStream.Length > _options.MaxReceiptSizeBytes)
        {
            _logger.LogWarning("Oversized file upload attempted: {Size} bytes (max: {Max})",
                fileStream.Length, _options.MaxReceiptSizeBytes);
            throw new InvalidOperationException($"حجم الملف يتجاوز الحد المسموح ({_options.MaxReceiptSizeMb} ميجابايت).");
        }

        var extension = Path.GetExtension(originalFileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !_options.AllowedExtensions.Contains(extension))
        {
            _logger.LogWarning("Disallowed file extension upload attempted: {Ext}", extension);
            throw new InvalidOperationException("نوع الملف غير مسموح. الأنواع المسموحة: " + string.Join(", ", _options.AllowedExtensions));
        }

        // Safe file naming with GUID
        var safeFileName = $"{Guid.NewGuid()}{extension}";
        var relativePath = Path.Combine(_options.ReceiptsPath, safeFileName);
        var fullPath = Path.Combine(_basePath, safeFileName);

        try
        {
            await using var fileStreamOut = new FileStream(fullPath, FileMode.Create);
            await fileStream.CopyToAsync(fileStreamOut, cancellationToken);

            _logger.LogInformation("Receipt saved: {FileName} ({Size} bytes)", safeFileName, fileStream.Length);
            return relativePath;
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Failed to save receipt file to disk: {Path}", safeFileName);
            throw new InvalidOperationException("فشل في حفظ الملف. يرجى المحاولة مرة أخرى.", ex);
        }
    }

    public void DeleteReceipt(string relativePath)
    {
        try
        {
            var fullPath = GetFullPath(relativePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Receipt deleted: {FileName}", Path.GetFileName(relativePath));
            }
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "Failed to delete receipt file: {FileName}", Path.GetFileName(relativePath));
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
