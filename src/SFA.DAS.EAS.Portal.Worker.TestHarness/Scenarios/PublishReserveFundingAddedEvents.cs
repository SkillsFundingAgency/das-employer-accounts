﻿using System;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.EAS.Portal.Events.Reservations;

namespace SFA.DAS.EAS.Portal.Worker.TestHarness.Scenarios
{
    public class PublishReserveFundingAddedEvents
    {
        private readonly IMessageSession _messageSession;

        public PublishReserveFundingAddedEvents(IMessageSession messageSession)
        {
            _messageSession = messageSession;
        }

        public async Task Run()
        {
            const long accountId = 1337L;
            const long accountLegalEntityId1 = 420L;
            const string legalEntityName1 = "Fishy Fingers Ltd";
            const long accountLegalEntityId2 = 8008135L;
            const string legalEntityName2 = "Ann Chovy's Fish Emporium Ltd";

            await _messageSession.Publish(new ReserveFundingAddedEvent
            {
                AccountId = accountId,
                AccountLegalEntityId = accountLegalEntityId1,
                LegalEntityName = legalEntityName1,
                CourseId = 3,
                CourseName = "Fish Monger, Level 3 (Standard)",
                StartDate = new DateTime(2020, 1, 1),
                EndDate = new DateTime(2021, 1, 1),
                Created = DateTime.UtcNow
            });

            // another reservation, same account, same legal entity
            await _messageSession.Publish(new ReserveFundingAddedEvent
            {
                AccountId = accountId,
                AccountLegalEntityId = accountLegalEntityId1,
                LegalEntityName = legalEntityName1,
                CourseId = 4,
                CourseName = "Fish Monger, Level 4 (Standard)",
                StartDate = new DateTime(2020, 2, 1),
                EndDate = new DateTime(2021, 2, 1),
                Created = DateTime.UtcNow
            });
            
            // another reservation, same account, differnt legal entity
            await _messageSession.Publish(new ReserveFundingAddedEvent
            {
                AccountId = accountId,
                AccountLegalEntityId = accountLegalEntityId2,
                LegalEntityName = legalEntityName2,
                CourseId = 2,
                CourseName = "Fish Monger, Level 2 (Standard)",
                StartDate = new DateTime(2020, 2, 1),
                EndDate = new DateTime(2021, 2, 1),
                Created = DateTime.UtcNow
            });
        }
    }
}