using JudgesTournament.Application.DTOs;

namespace JudgesTournament.Web.ViewModels.Admin;

public class RegistrationListViewModel
{
    public PagedResult<RegistrationListItemDto> PagedResult { get; set; } = new();
    public RegistrationFilterDto Filter { get; set; } = new();
}
