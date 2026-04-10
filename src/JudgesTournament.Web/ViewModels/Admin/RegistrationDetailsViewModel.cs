using JudgesTournament.Application.DTOs;
using JudgesTournament.Domain.Enums;

namespace JudgesTournament.Web.ViewModels.Admin;

public class RegistrationDetailsViewModel
{
    public RegistrationDetailsDto Registration { get; set; } = new();
    public RegistrationStatus NewStatus { get; set; }
    public string? AdminNotes { get; set; }
}
