namespace JudgesTournament.Application.Options;

public class RegistrationOptions
{
    public const string SectionName = "Registration";

    public bool IsOpen { get; set; } = true;
    public decimal Fee { get; set; } = 50;
    public int MaxPlayersPerTeam { get; set; } = 25;
}
