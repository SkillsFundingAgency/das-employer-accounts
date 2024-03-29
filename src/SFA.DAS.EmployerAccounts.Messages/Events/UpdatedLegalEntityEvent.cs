﻿using System;

namespace SFA.DAS.EmployerAccounts.Messages.Events
{
    public class UpdatedLegalEntityEvent 
    {
        public long AccountLegalEntityId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string UserName { get; set; }
        public Guid UserRef { get; set; }

        public string OrganisationName { get; set; }
        public DateTime Created { get; set; }
    }
}
