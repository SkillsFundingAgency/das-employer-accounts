using System;
using SFA.DAS.Common.Domain.Types;

namespace SFA.DAS.EmployerAccounts.Api.Types
{
    public class AccountDetail
    {
        public long AccountId { get; set; }
        public string HashedAccountId { get; set; }
        public string PublicHashedAccountId { get; set; }
        public string DasAccountName { get; set; }
        public DateTime DateRegistered { get; set; }
        public string OwnerEmail { get; set; }
        public ResourceList LegalEntities { get; set; }
        public ResourceList PayeSchemes { get; set; }
        public bool? NameConfirmed { get; set; }

        [Obsolete]
        public string DasAccountId => HashedAccountId;

        public AccountAgreementType AccountAgreementType { get; set; }	
        public string ApprenticeshipEmployerType { get; set; }
        public ApprenticeshipEmployerType EmployerType { get; set; }
        public bool? AddTrainingProviderAcknowledged { get; set; }
    }
}
