namespace JudgesTournament.Application.Options;

public class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public int PermitLimit { get; set; } = 5;
    public int WindowMinutes { get; set; } = 15;
}
