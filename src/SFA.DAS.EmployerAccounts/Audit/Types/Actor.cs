﻿namespace SFA.DAS.EmployerAccounts.Audit.Types;
public sealed class Actor
{
    public string Id { get; set; }
    public string EmailAddress { get; set; }
    public string OriginIpAddress { get; set; }
}
