﻿using System;
using SFA.DAS.EmployerAccounts.Types.Models;

namespace SFA.DAS.EmployerAccounts.Messages.Events
{
    public class AddedLegalEntityEvent
    {
        public long AccountId { get; set; }
        public string UserName { get; set; }
        public Guid UserRef { get; set; }
        public string OrganisationName { get; set; }
        public long AgreementId { get; set; }
        public long LegalEntityId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public OrganisationType OrganisationType { get; set; }
        public string OrganisationReferenceNumber { get; set; }
        public string OrganisationAddress { get; set; }
        public DateTime Created { get; set; }
    }
}
