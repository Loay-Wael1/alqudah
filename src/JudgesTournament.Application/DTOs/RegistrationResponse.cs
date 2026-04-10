namespace JudgesTournament.Application.DTOs;

public class RegistrationResponse
{
    public bool Success { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? ErrorMessage { get; set; }

    public static RegistrationResponse Ok(string referenceNumber) =>
        new() { Success = true, ReferenceNumber = referenceNumber };

    public static RegistrationResponse Fail(string errorMessage) =>
        new() { Success = false, ErrorMessage = errorMessage };
}
