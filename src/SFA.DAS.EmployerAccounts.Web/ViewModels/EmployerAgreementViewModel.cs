﻿namespace SFA.DAS.EmployerAccounts.Web.ViewModels;

public class EmployerAgreementViewModel
{
    public EmployerAgreementView EmployerAgreement { get; set; }

    /// <summary>
    ///     Indicates whether the organisation that signed the agreement can be looked up by id
    ///     in reference data. e.g. if the organisation that signed the agreement were a
    ///     company then the company could be looked up using its company number. Conversely, 
    ///     if the organisation were a public sector body then it would not be possible to look
    ///     up the company.
    /// </summary>
    public bool OrganisationLookupPossible { get; set; }

    public bool NoChoiceSelected { get; set; }

    public int LegalEntitiesCount { get; set; }

    public EmployerAgreementStatus Status { get; set; }
    public bool IsFromTasklist { get; set; }
}