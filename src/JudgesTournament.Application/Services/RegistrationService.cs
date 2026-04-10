using JudgesTournament.Application.DTOs;
using JudgesTournament.Application.Helpers;
using JudgesTournament.Application.Interfaces;
using JudgesTournament.Application.Options;
using JudgesTournament.Domain.Constants;
using JudgesTournament.Domain.Entities;
using JudgesTournament.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JudgesTournament.Application.Services;

public class RegistrationService : IRegistrationService
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<RegistrationService> _logger;
    private readonly RegistrationOptions _regOptions;

    public RegistrationService(IApplicationDbContext db, ILogger<RegistrationService> logger, IOptions<RegistrationOptions> regOptions)
    {
        _db = db;
        _logger = logger;
        _regOptions = regOptions.Value;
    }

    public async Task<RegistrationResponse> CreateAsync(CreateRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        // === Business validation ===

        // Governorate must be in the official list
        if (!Governorates.All.Contains(request.Governorate))
            return RegistrationResponse.Fail("المحافظة المختارة غير صالحة.");

        // TransferAmount must equal the configured fee
        if (request.TransferAmount != _regOptions.Fee)
            return RegistrationResponse.Fail($"قيمة التحويل يجب أن تكون {_regOptions.Fee} جنيه.");

        // TransferDate must not be in the future
        if (request.TransferDate.Date > DateTime.UtcNow.Date)
            return RegistrationResponse.Fail("تاريخ التحويل لا يمكن أن يكون في المستقبل.");

        // === Normalize inputs ===
        var normalizedName = TeamNameNormalizer.Normalize(request.TeamName);
        var normalizedPhone = PhoneNormalizer.Normalize(request.PhoneNumber);
        var normalizedWhatsApp = PhoneNormalizer.Normalize(request.WhatsAppNumber);
        var normalizedTransferFrom = PhoneNormalizer.Normalize(request.TransferFromNumber);

        if (normalizedPhone.Length < 10)
            return RegistrationResponse.Fail("رقم الهاتف غير صالح.");

        // === Duplicate pre-checks ===
        var phoneExists = await _db.TeamRegistrations
            .AsNoTracking()
            .AnyAsync(r => r.PhoneNumber == normalizedPhone, cancellationToken);

        if (phoneExists)
        {
            _logger.LogInformation("Duplicate phone registration attempt: {Phone}", normalizedPhone);
            return RegistrationResponse.Fail("رقم الهاتف مسجل بالفعل. لا يمكن التسجيل بنفس الرقم أكثر من مرة.");
        }

        var teamExists = await _db.TeamRegistrations
            .AsNoTracking()
            .AnyAsync(r => r.NormalizedTeamName == normalizedName && r.Governorate == request.Governorate, cancellationToken);

        if (teamExists)
        {
            _logger.LogInformation("Duplicate team registration attempt: {Team} in {Gov}", request.TeamName, request.Governorate);
            return RegistrationResponse.Fail("هذا الفريق مسجل بالفعل في نفس المحافظة.");
        }

        // === Create entity ===
        var entity = new TeamRegistration
        {
            TeamName = request.TeamName.Trim(),
            NormalizedTeamName = normalizedName,
            Governorate = request.Governorate,
            PlayersCount = request.PlayersCount,
            UniformColor = request.UniformColor.Trim(),
            ContactPersonName = request.ContactPersonName.Trim(),
            PhoneNumber = normalizedPhone,
            WhatsAppNumber = normalizedWhatsApp,
            TransferFromNumber = normalizedTransferFrom,
            TransferName = request.TransferName.Trim(),
            TransferAmount = request.TransferAmount,
            TransferDate = request.TransferDate,
            ReceiptImagePath = request.ReceiptImagePath,
            AgreedToTerms = request.AgreedToTerms,
            ConfirmedPreliminary = request.ConfirmedPreliminary,
            Status = RegistrationStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };

        // Use transaction to ensure entity + reference number are saved atomically
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            _db.TeamRegistrations.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            // Generate deterministic reference number using DB-assigned Id
            entity.ReferenceNumber = $"QJ-2026-{entity.Id:D6}";
            await _db.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Registration created: {Ref} for team {Team} from {Gov}",
                entity.ReferenceNumber, entity.TeamName, entity.Governorate);

            return RegistrationResponse.Ok(entity.ReferenceNumber);
        }
        catch (DbUpdateException ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync(cancellationToken);

            var friendlyMessage = DuplicateExceptionHandler.GetFriendlyMessage(ex);
            if (friendlyMessage is not null)
            {
                _logger.LogWarning("Duplicate registration caught at DB level for team {Team}: {Msg}",
                    request.TeamName, friendlyMessage);
                return RegistrationResponse.Fail(friendlyMessage);
            }

            _logger.LogError(ex, "Database error during registration for team {Team}", request.TeamName);
            throw;
        }
        finally
        {
            if (transaction is not null)
                await transaction.DisposeAsync();
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
        {
            _logger.LogWarning("Status update attempted on non-existent registration {Id}", request.RegistrationId);
            return (false, "الطلب غير موجود.");
        }

        // Set the original RowVersion for concurrency check
        _db.Entry(registration).Property(r => r.RowVersion).OriginalValue = request.RowVersion;

        var oldStatus = registration.Status;
        var statusChanged = oldStatus != request.NewStatus;

        // Validate status transition if status is actually changing
        if (statusChanged)
        {
            var transitionError = StatusTransitionRules.Validate(oldStatus, request.NewStatus);
            if (transitionError is not null)
            {
                _logger.LogWarning("Invalid status transition attempted: {Old} → {New} for registration {Id}",
                    oldStatus, request.NewStatus, registration.Id);
                return (false, transitionError);
            }

            registration.Status = request.NewStatus;

            // Record status change in audit trail
            _db.RegistrationStatusHistories.Add(new RegistrationStatusHistory
            {
                TeamRegistrationId = registration.Id,
                OldStatus = oldStatus,
                NewStatus = request.NewStatus,
                ChangedByAdminId = request.AdminUserId,
                ChangedAtUtc = DateTime.UtcNow,
                Notes = request.AdminNotes
            });
        }

        // Always update admin notes and review tracking
        registration.AdminNotes = request.AdminNotes;
        registration.ReviewedAtUtc = DateTime.UtcNow;
        registration.ReviewedByAdminId = request.AdminUserId;

        try
        {
            await _db.SaveChangesAsync(cancellationToken);

            if (statusChanged)
            {
                _logger.LogInformation("Registration {Id} ({Ref}) status: {Old} → {New} by admin {Admin}",
                    registration.Id, registration.ReferenceNumber, oldStatus, request.NewStatus, request.AdminUserId);
            }
            else
            {
                _logger.LogInformation("Registration {Id} ({Ref}) notes updated by admin {Admin}",
                    registration.Id, registration.ReferenceNumber, request.AdminUserId);
            }

            return (true, null);
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning("Concurrency conflict on registration {Id} ({Ref}) by admin {Admin}",
                registration.Id, registration.ReferenceNumber, request.AdminUserId);
            return (false, "تم تعديل هذا الطلب من قبل مستخدم آخر. يرجى إعادة تحميل الصفحة والمحاولة مرة أخرى.");
        }
    }
}
