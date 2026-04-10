namespace JudgesTournament.Application.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Saves an uploaded file to disk after validating extension and size.
    /// Returns the relative path to the saved file.
    /// </summary>
    Task<string> SaveReceiptAsync(Stream fileStream, string originalFileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file from disk.
    /// </summary>
    void DeleteReceipt(string relativePath);

    /// <summary>
    /// Gets the full filesystem path for serving.
    /// </summary>
    string GetFullPath(string relativePath);
}
