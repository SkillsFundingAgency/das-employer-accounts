using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Polly;
using Polly.Registry;
using Polly.Timeout;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Models.Recruit;
using SFA.DAS.EmployerAccounts.Services;

namespace SFA.DAS.EmployerAccounts.UnitTests.Services.Recruit
{
    public class WhenIGetVacanciesWithTimeout
    {
        private Mock<IRecruitService> _mockRecruitService;
        private IAsyncPolicy _policy;
        private long _accountId;
        private RecruitServiceWithTimeout _recruitServiceWithTimeout;
        private Vacancy _vacancy = new()
        {
            Title = "Vacancy 1",
        };

        [SetUp]
        public void Arrange()
        {
            _accountId = 20002;
            _mockRecruitService = new Mock<IRecruitService>();

            _mockRecruitService.Setup(rs => rs.GetVacancies(_accountId))
                .ReturnsAsync(_vacancy);
                
            _policy = Policy.NoOpAsync();
            var registryPolicy = new PolicyRegistry
            {
                { Constants.DefaultServiceTimeout, _policy }
            };

            _recruitServiceWithTimeout = new RecruitServiceWithTimeout(_mockRecruitService.Object, registryPolicy);
        }

        [Test]
        public async Task ThenTheRecruitServiceIsCalled()
        {
            //act
            await _recruitServiceWithTimeout.GetVacancies(_accountId);

            // assert 
            _mockRecruitService.Verify(x => x.GetVacancies(_accountId), Times.Once);
        }

        [Test]
        public async Task ThenTheRecruitServiceReturnsTheSameReservation()
        {
            //act
            var recruitsResult = await _recruitServiceWithTimeout.GetVacancies(_accountId);

            // assert 
            Assert.That(_vacancy, Is.SameAs(recruitsResult));
        }

        [Test]
        public async Task ThenThrowTimeoutException()
        {
            var innerException = "Exception of type 'Polly.Timeout.TimeoutRejectedException' was thrown.";
            var message = "Call to Recruit Service timed out";
            Exception actualException = null;
            var correctExceptionThrown = false;

            try
            {
                _mockRecruitService.Setup(p => p.GetVacancies(_accountId))
                    .Throws<TimeoutRejectedException>();
                await _recruitServiceWithTimeout.GetVacancies(_accountId);
            }
            catch (Exception e)
            {
                actualException = e;
                correctExceptionThrown = true;
            }
            Assert.That(correctExceptionThrown, Is.True);
            Assert.That(innerException, Is.EqualTo(actualException.InnerException?.Message));
            Assert.That(message, Is.EqualTo(actualException.Message));
        }
    }
}
