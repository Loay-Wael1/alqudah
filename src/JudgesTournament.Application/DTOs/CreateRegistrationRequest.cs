namespace JudgesTournament.Application.DTOs;

public class CreateRegistrationRequest
{
    public string TeamName { get; set; } = string.Empty;
    public string Governorate { get; set; } = string.Empty;
    public int PlayersCount { get; set; }
    public string UniformColor { get; set; } = string.Empty;
    public string ContactPersonName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string WhatsAppNumber { get; set; } = string.Empty;
    public string TransferFromNumber { get; set; } = string.Empty;
    public string TransferName { get; set; } = string.Empty;
    public decimal TransferAmount { get; set; }
    public DateTime TransferDate { get; set; }
    public string ReceiptImagePath { get; set; } = string.Empty;
    public bool AgreedToTerms { get; set; }
    public bool ConfirmedPreliminary { get; set; }
}
