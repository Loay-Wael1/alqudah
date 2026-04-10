using JudgesTournament.Domain.Enums;

namespace JudgesTournament.Application.DTOs;

public class RegistrationFilterDto
{
    public string? SearchTerm { get; set; }
    public RegistrationStatus? Status { get; set; }
    public string? Governorate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
