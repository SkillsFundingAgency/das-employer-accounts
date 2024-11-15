using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Commands.AccountLevyStatus;
using SFA.DAS.EmployerAccounts.MessageHandlers.EventHandlers.EmployerFinance;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Testing;

namespace SFA.DAS.EmployerAccounts.MessageHandlers.UnitTests.EventHandlers.EmployerFinance
{
    [TestFixture]
    public class RefreshEmployerLevyDataCompletedEventHandlerTests : FluentTest<RefreshEmployerLevyDataCompletedEventHandlerTestsFixture>
    {

        [TestCase(0, ApprenticeshipEmployerType.NonLevy)]
        [TestCase(100, ApprenticeshipEmployerType.Levy)]
        public Task WhenMessageIsHandled_AccountLevyStatusCommandIsSent(decimal levyValue, ApprenticeshipEmployerType apprenticeshipEmployerType)
        {
            var timestamp = DateTime.UtcNow;
            const long accountId = 666;
            const bool levyImported = true;
            const short periodMonth = 7;
            const string periodYear = "2018";

            return TestAsync(f => f.Handle(new RefreshEmployerLevyDataCompletedEvent
            {
                AccountId = accountId,
                LevyImported = levyImported,
                PeriodMonth = periodMonth,
                PeriodYear = periodYear,
                LevyTransactionValue = levyValue,
                Created = timestamp
            })
                , (f) =>
                {
                    f.VerifyAccountLevyStatusCommandIsSent(accountId, apprenticeshipEmployerType);
                });
        }
    }

    public class RefreshEmployerLevyDataCompletedEventHandlerTestsFixture
    {
        private readonly RefreshEmployerLevyDataCompletedEventHandler _handler;
        private readonly Mock<IMediator> _mediator;

        public RefreshEmployerLevyDataCompletedEventHandlerTestsFixture()
        {
            _mediator = new Mock<IMediator>();

            _handler = new RefreshEmployerLevyDataCompletedEventHandler(_mediator.Object);
        }

        public Task Handle(RefreshEmployerLevyDataCompletedEvent refreshEmployerLevyDataCompletedEvent)
        {
            return _handler.Handle(refreshEmployerLevyDataCompletedEvent, null);
        }

        public void VerifyAccountLevyStatusCommandIsSent(long accountId, ApprenticeshipEmployerType apprenticeshipEmployerType)
        {
            _mediator.Verify(e => e.Send(It.Is<AccountLevyStatusCommand>(m =>
                m.AccountId.Equals(accountId) &&
                m.ApprenticeshipEmployerType.Equals(apprenticeshipEmployerType)), CancellationToken.None),
                Times.Once);
        }
    }
}