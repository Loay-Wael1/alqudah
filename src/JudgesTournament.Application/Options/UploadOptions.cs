namespace JudgesTournament.Application.Options;

public class UploadOptions
{
    public const string SectionName = "Upload";

    public int MaxReceiptSizeMb { get; set; } = 2;
    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp"];
    public string ReceiptsPath { get; set; } = "uploads/receipts";

    public long MaxReceiptSizeBytes => MaxReceiptSizeMb * 1024L * 1024L;
}
