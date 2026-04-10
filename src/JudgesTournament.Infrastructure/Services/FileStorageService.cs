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

    // Magic bytes for image validation
    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] WebpSignature = [0x52, 0x49, 0x46, 0x46]; // RIFF

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

        // Magic bytes validation — verify file content matches claimed extension
        if (!await IsValidImageAsync(fileStream, extension))
        {
            _logger.LogWarning("File signature mismatch: claimed {Ext}, actual content differs", extension);
            throw new InvalidOperationException("محتوى الملف لا يتطابق مع نوعه. يرجى رفع صورة صالحة.");
        }

        // Reset stream position after reading magic bytes
        fileStream.Position = 0;

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
            _logger.LogError(ex, "Failed to save receipt file to disk: {FileName}", safeFileName);
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

    /// <summary>
    /// Validates file content by checking magic bytes match the claimed extension.
    /// </summary>
    private static async Task<bool> IsValidImageAsync(Stream stream, string extension)
    {
        if (!stream.CanSeek) return true; // Can't validate non-seekable streams, allow through

        stream.Position = 0;
        var header = new byte[8];
        var bytesRead = await stream.ReadAsync(header.AsMemory(0, 8));

        if (bytesRead < 3) return false;

        return extension switch
        {
            ".jpg" or ".jpeg" => header[0] == JpegSignature[0] && header[1] == JpegSignature[1] && header[2] == JpegSignature[2],
            ".png" => bytesRead >= 8 && header.AsSpan(0, 8).SequenceEqual(PngSignature),
            ".webp" => bytesRead >= 4 && header.AsSpan(0, 4).SequenceEqual(WebpSignature),
            _ => false
        };
    }
}

/// <summary>
/// Simple accessor to provide ContentRootPath to FileStorageService without depending on IWebHostEnvironment directly in Infrastructure.
/// </summary>
public interface IWebHostEnvironmentAccessor
{
    string ContentRootPath { get; }
}
