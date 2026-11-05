namespace SFA.DAS.EmployerAccounts.Configuration;

public sealed record Features
{
    public bool LevyProjectionTransparency { get; init; } = false;
}