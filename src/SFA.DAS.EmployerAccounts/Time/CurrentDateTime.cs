namespace SFA.DAS.EmployerAccounts.Time;

public sealed class CurrentDateTime : ICurrentDateTime
{
    private readonly DateTime? _fixed;

    public DateTime Now => _fixed ?? DateTime.Now;

    public CurrentDateTime()
    {
    }

    public CurrentDateTime(DateTime time)
    {
        _fixed = time;
    }
}
