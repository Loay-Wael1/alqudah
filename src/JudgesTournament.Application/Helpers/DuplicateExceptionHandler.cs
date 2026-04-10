using Microsoft.EntityFrameworkCore;

namespace JudgesTournament.Application.Helpers;

/// <summary>
/// Translates database unique constraint violations into user-friendly Arabic messages.
/// </summary>
public static class DuplicateExceptionHandler
{
    public static string? GetFriendlyMessage(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message ?? ex.Message;

        if (message.Contains("IX_TeamRegistrations_PhoneNumber", StringComparison.OrdinalIgnoreCase))
            return "رقم الهاتف مسجل بالفعل. لا يمكن التسجيل بنفس الرقم أكثر من مرة.";

        if (message.Contains("IX_TeamRegistrations_NormalizedTeamName_Governorate", StringComparison.OrdinalIgnoreCase))
            return "هذا الفريق مسجل بالفعل في نفس المحافظة.";

        return null;
    }
}
