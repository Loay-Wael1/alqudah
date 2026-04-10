using JudgesTournament.Domain.Enums;

namespace JudgesTournament.Application.Helpers;

/// <summary>
/// Enforces allowed status transitions for registrations.
/// Prevents arbitrary state changes that could corrupt the workflow.
/// </summary>
public static class StatusTransitionRules
{
    private static readonly Dictionary<RegistrationStatus, HashSet<RegistrationStatus>> AllowedTransitions = new()
    {
        [RegistrationStatus.Pending] = [RegistrationStatus.PaymentVerified, RegistrationStatus.Rejected],
        [RegistrationStatus.PaymentVerified] = [RegistrationStatus.Accepted, RegistrationStatus.Rejected],
        [RegistrationStatus.Accepted] = [],      // Final state — no further transitions
        [RegistrationStatus.Rejected] = [],       // Final state — no further transitions
    };

    /// <summary>
    /// Returns true if transitioning from <paramref name="current"/> to <paramref name="target"/> is allowed.
    /// </summary>
    public static bool IsAllowed(RegistrationStatus current, RegistrationStatus target)
    {
        if (current == target) return true; // No-op is always allowed (handled separately)
        return AllowedTransitions.TryGetValue(current, out var targets) && targets.Contains(target);
    }

    /// <summary>
    /// Returns a user-friendly Arabic error message if transition is not allowed, or null if it is.
    /// </summary>
    public static string? Validate(RegistrationStatus current, RegistrationStatus target)
    {
        if (current == target) return null;

        if (!IsAllowed(current, target))
        {
            var currentAr = GetArabicName(current);
            var targetAr = GetArabicName(target);
            return $"لا يمكن تغيير الحالة من \"{currentAr}\" إلى \"{targetAr}\".";
        }

        return null;
    }

    private static string GetArabicName(RegistrationStatus status) => status switch
    {
        RegistrationStatus.Pending => "قيد المراجعة",
        RegistrationStatus.PaymentVerified => "تم التحقق من السداد",
        RegistrationStatus.Accepted => "مقبول",
        RegistrationStatus.Rejected => "مرفوض",
        _ => status.ToString()
    };
}
