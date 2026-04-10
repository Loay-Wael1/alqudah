using JudgesTournament.Application.DTOs;
using JudgesTournament.Application.Helpers;
using JudgesTournament.Application.Interfaces;
using JudgesTournament.Domain.Entities;
using JudgesTournament.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JudgesTournament.Application.Services;

public class RegistrationService : IRegistrationService
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<RegistrationService> _logger;

    public RegistrationService(IApplicationDbContext db, ILogger<RegistrationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<RegistrationResponse> CreateAsync(CreateRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedName = TeamNameNormalizer.Normalize(request.TeamName);

        // Application-level duplicate pre-check
        var phoneExists = await _db.TeamRegistrations
            .AsNoTracking()
            .AnyAsync(r => r.PhoneNumber == request.PhoneNumber, cancellationToken);

        if (phoneExists)
            return RegistrationResponse.Fail("رقم الهاتف مسجل بالفعل. لا يمكن التسجيل بنفس الرقم أكثر من مرة.");

        var teamExists = await _db.TeamRegistrations
            .AsNoTracking()
            .AnyAsync(r => r.NormalizedTeamName == normalizedName && r.Governorate == request.Governorate, cancellationToken);

        if (teamExists)
            return RegistrationResponse.Fail("هذا الفريق مسجل بالفعل في نفس المحافظة.");

        var entity = new TeamRegistration
        {
            TeamName = request.TeamName.Trim(),
            NormalizedTeamName = normalizedName,
            Governorate = request.Governorate,
            PlayersCount = request.PlayersCount,
            UniformColor = request.UniformColor.Trim(),
            ContactPersonName = request.ContactPersonName.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            WhatsAppNumber = request.WhatsAppNumber.Trim(),
            TransferFromNumber = request.TransferFromNumber.Trim(),
            TransferName = request.TransferName.Trim(),
            TransferAmount = request.TransferAmount,
            TransferDate = request.TransferDate,
            ReceiptImagePath = request.ReceiptImagePath,
            AgreedToTerms = request.AgreedToTerms,
            ConfirmedPreliminary = request.ConfirmedPreliminary,
            Status = RegistrationStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };

        try
        {
            _db.TeamRegistrations.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            // Generate deterministic reference number using DB-assigned Id
            entity.ReferenceNumber = $"QJ-2026-{entity.Id:D6}";
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Registration created: {ReferenceNumber} for team {TeamName}",
                entity.ReferenceNumber, entity.TeamName);

            return RegistrationResponse.Ok(entity.ReferenceNumber);
        }
        catch (DbUpdateException ex)
        {
            var friendlyMessage = DuplicateExceptionHandler.GetFriendlyMessage(ex);
            if (friendlyMessage is not null)
            {
                _logger.LogWarning("Duplicate registration attempt caught at DB level: {Message}", friendlyMessage);
                return RegistrationResponse.Fail(friendlyMessage);
            }

            _logger.LogError(ex, "Database error during registration creation");
            throw;
        }
    }

    public async Task<RegistrationDetailsDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.TeamRegistrations
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new RegistrationDetailsDto
            {
                Id = r.Id,
                TeamName = r.TeamName,
                Governorate = r.Governorate,
                PlayersCount = r.PlayersCount,
                UniformColor = r.UniformColor,
                ContactPersonName = r.ContactPersonName,
                PhoneNumber = r.PhoneNumber,
                WhatsAppNumber = r.WhatsAppNumber,
                TransferFromNumber = r.TransferFromNumber,
                TransferName = r.TransferName,
                TransferAmount = r.TransferAmount,
                TransferDate = r.TransferDate,
                ReceiptImagePath = r.ReceiptImagePath,
                ReferenceNumber = r.ReferenceNumber,
                Status = r.Status,
                AdminNotes = r.AdminNotes,
                CreatedAtUtc = r.CreatedAtUtc,
                RowVersion = r.RowVersion,
                StatusHistory = r.StatusHistory
                    .OrderByDescending(h => h.ChangedAtUtc)
                    .Select(h => new StatusHistoryItemDto
                    {
                        OldStatus = h.OldStatus,
                        NewStatus = h.NewStatus,
                        ChangedByAdminId = h.ChangedByAdminId,
                        ChangedAtUtc = h.ChangedAtUtc,
                        Notes = h.Notes
                    }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<RegistrationListItemDto>> GetPagedAsync(RegistrationFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _db.TeamRegistrations.AsNoTracking().AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(r =>
                r.TeamName.Contains(term) ||
                r.PhoneNumber.Contains(term) ||
                r.ContactPersonName.Contains(term) ||
                (r.ReferenceNumber != null && r.ReferenceNumber.Contains(term)));
        }

        if (filter.Status.HasValue)
            query = query.Where(r => r.Status == filter.Status.Value);

        if (!string.IsNullOrWhiteSpace(filter.Governorate))
            query = query.Where(r => r.Governorate == filter.Governorate);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.CreatedAtUtc)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(r => new RegistrationListItemDto
            {
                Id = r.Id,
                TeamName = r.TeamName,
                Governorate = r.Governorate,
                PlayersCount = r.PlayersCount,
                ContactPersonName = r.ContactPersonName,
                PhoneNumber = r.PhoneNumber,
                ReferenceNumber = r.ReferenceNumber,
                Status = r.Status,
                CreatedAtUtc = r.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<RegistrationListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var stats = await _db.TeamRegistrations
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new DashboardStatsDto
            {
                TotalRegistrations = g.Count(),
                PendingCount = g.Count(r => r.Status == RegistrationStatus.Pending),
                PaymentVerifiedCount = g.Count(r => r.Status == RegistrationStatus.PaymentVerified),
                AcceptedCount = g.Count(r => r.Status == RegistrationStatus.Accepted),
                RejectedCount = g.Count(r => r.Status == RegistrationStatus.Rejected),
                TotalTransferAmount = g.Sum(r => r.TransferAmount)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return stats ?? new DashboardStatsDto();
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateStatusAsync(UpdateRegistrationStatusRequest request, CancellationToken cancellationToken = default)
    {
        var registration = await _db.TeamRegistrations
            .FirstOrDefaultAsync(r => r.Id == request.RegistrationId, cancellationToken);

        if (registration is null)
            return (false, "الطلب غير موجود.");

        // Set the original RowVersion for concurrency check
        _db.TeamRegistrations.Entry(registration).Property(r => r.RowVersion).OriginalValue = request.RowVersion;

        var oldStatus = registration.Status;
        registration.Status = request.NewStatus;
        registration.AdminNotes = request.AdminNotes;

        // Record status change in audit trail
        var history = new RegistrationStatusHistory
        {
            TeamRegistrationId = registration.Id,
            OldStatus = oldStatus,
            NewStatus = request.NewStatus,
            ChangedByAdminId = request.AdminUserId,
            ChangedAtUtc = DateTime.UtcNow,
            Notes = request.AdminNotes
        };

        _db.RegistrationStatusHistories.Add(history);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Registration {Id} status changed from {Old} to {New} by {Admin}",
                registration.Id, oldStatus, request.NewStatus, request.AdminUserId);
            return (true, null);
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning("Concurrency conflict on registration {Id}", registration.Id);
            return (false, "تم تعديل هذا الطلب من قبل مستخدم آخر. يرجى إعادة تحميل الصفحة والمحاولة مرة أخرى.");
        }
    }
}
