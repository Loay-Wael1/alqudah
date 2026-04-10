using JudgesTournament.Application.DTOs;

namespace JudgesTournament.Application.Interfaces;

public interface IRegistrationService
{
    Task<RegistrationResponse> CreateAsync(CreateRegistrationRequest request, CancellationToken cancellationToken = default);
    Task<RegistrationDetailsDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<string?> GetReceiptPathByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<RegistrationListItemDto>> GetPagedAsync(RegistrationFilterDto filter, CancellationToken cancellationToken = default);
    Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage)> UpdateStatusAsync(UpdateRegistrationStatusRequest request, CancellationToken cancellationToken = default);
}
