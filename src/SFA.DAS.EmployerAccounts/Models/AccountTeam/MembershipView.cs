﻿namespace SFA.DAS.EmployerAccounts.Models.AccountTeam;

public class MembershipView
{
    public string HashedAccountId { get; set; }
    public long AccountId { get; set; }
    public string AccountName { get; set; }
    public long UserId { get; set; }
    public Guid UserRef { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string CorrelationId { get; set; }
    public Role Role { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool ShowWizard { get; set; }
    public string FullName() => $"{FirstName} {LastName}";
}