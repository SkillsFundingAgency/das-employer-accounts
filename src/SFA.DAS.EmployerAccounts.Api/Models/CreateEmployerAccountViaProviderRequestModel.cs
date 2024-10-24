using System;

namespace SFA.DAS.EmployerAccounts.Api;

public class CreateEmployerAccountViaProviderRequestModel
{
    /// <summary>
    ///  This is originated in Provider Relationships
    ///  and will be used as CorrelationId in here
    /// </summary>
    public string RequestId { get; set; }
    public Guid UserRef { get; set; }
    public string Email { get; set; }
    public string EmployerName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmployerOrganisationName { get; set; }
    public string EmployerPaye { get; set; }
    public string EmployerAorn { get; set; }
    public string EmployerAddress { get; set; }
    public string EmployerOrganisationReferenceNumber { get; set; }
}
