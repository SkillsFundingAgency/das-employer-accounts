﻿namespace SFA.DAS.EmployerAccounts.Configuration;

public class ProviderRegistrationsConfiguration
{
    public string BaseUrl { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string IdentifierUri { get; set; }   
    public string Tenant { get; set; }
}