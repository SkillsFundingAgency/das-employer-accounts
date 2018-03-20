﻿using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Domain.Models.AccountTeam;
using SFA.DAS.EAS.Domain.Models.Authorization;
using SFA.DAS.EAS.Domain.Models.FeatureToggles;
using SFA.DAS.EAS.Domain.Models.UserProfile;
using SFA.DAS.EAS.Infrastructure.Caching;
using SFA.DAS.EAS.Infrastructure.Services;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EAS.Infrastructure.UnitTests.Services.FeatureToggleServiceTests
{
    [TestFixture]
    public class WhenICheckIfFeatureIsEnabled
    {
        private const string ControllerName = "Foo";
        private const string ActionName = "Bar";

        private Mock<ICacheProvider> _cacheProvider;
        private Mock<ILog> _logger;
        private Mock<FeatureToggleService> _featureToggleServiceMock;
        private FeatureToggleService _featureToggleService;
        private IMembershipContext _membershipContext;

        [SetUp]
        public void Arrange()
        {
            _membershipContext = new MembershipContext
            {
                UserId = 111111,
                UserEmail = "user.one@unit.tests"
            };

            _cacheProvider = new Mock<ICacheProvider>();
            _logger = new Mock<ILog>();

            _cacheProvider.SetupSequence(c => c.Get<FeatureToggleConfiguration>(nameof(FeatureToggleConfiguration))).Returns(null).Returns(new FeatureToggleConfiguration());

            _featureToggleServiceMock = new Mock<FeatureToggleService>(_cacheProvider.Object, _logger.Object);

            _featureToggleServiceMock.Setup(f => f.GetDataFromTableStorage()).Returns(new FeatureToggleConfiguration());
            _featureToggleServiceMock.Setup(f => f.IsFeatureEnabled(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IMembershipContext>())).CallBase();

            _featureToggleService = _featureToggleServiceMock.Object;
        }

        [Test]
        public void ThenShouldGetFeaturesFromStorageOnce()
        {
            //Act
            _featureToggleService.IsFeatureEnabled("", "", null);
            _featureToggleService.IsFeatureEnabled("", "", null);

            //Assert
            _featureToggleServiceMock.Verify(f => f.GetDataFromTableStorage(), Times.Once());
        }

        [Test]
        public void ThenShouldGetFeaturesFromCache()
        {
            //Act
            _featureToggleService.IsFeatureEnabled("", "", null);
            _featureToggleService.IsFeatureEnabled("", "", null);

            //Assert
            _cacheProvider.Verify(c => c.Get<FeatureToggleConfiguration>(nameof(FeatureToggleConfiguration)), Times.Exactly(2));
        }

        [Test]
        public void ThenShouldNotCacheFeatureIfEmpty()
        {
            //Arrange
            _featureToggleServiceMock.Setup(f => f.GetDataFromTableStorage()).Returns(new FeatureToggleConfiguration());

            //Act
            _featureToggleService.IsFeatureEnabled("", "", null);

            //Assert
            _cacheProvider.Verify(c => c.Set(nameof(FeatureToggleConfiguration),It.IsAny<FeatureToggleConfiguration>(), It.IsAny<TimeSpan>()), Times.Never);
        }

        [TestCase("user\\.one@unit\\.tests")]
        [TestCase("USER\\.ONE@UNIT\\.TESTS")]
        [TestCase(".*@unit\\.tests")]
        public void ThenShouldReturnTrueIfFeatureIsDisabledButUserIsInWhitelist(string whitelistPattern)
        {
            // Arrange
            _featureToggleServiceMock.Setup(f => f.GetDataFromTableStorage())
                .Returns(new FeatureToggleConfiguration
                {
                    Data = new List<FeatureToggle>
                    {
                        new FeatureToggle
                        {
                            Controller = ControllerName,
                            Action = ActionName,
                            Whitelist = new List<string> { whitelistPattern }
                        }
                    }
                });

            // Act
            var isFeatureEnabled = _featureToggleService.IsFeatureEnabled(ControllerName, ActionName, _membershipContext);

            // Assert
            Assert.That(isFeatureEnabled, Is.True);
        }

        [Test]
        public void ThenShouldReturnFalseIfFeatureIsDisabledAndUserIsNotInWhitelist()
        {
            // Arrange
            _featureToggleServiceMock.Setup(f => f.GetDataFromTableStorage())
                .Returns(new FeatureToggleConfiguration
                {
                    Data = new List<FeatureToggle>
                    {
                        new FeatureToggle
                        {
                            Controller = ControllerName,
                            Action = ActionName,
                            Whitelist = new List<string> { "different.user@somewhere.else" }
                        }
                    }
                });

            // Act
            var isFeatureEnabled = _featureToggleService.IsFeatureEnabled(ControllerName, ActionName, _membershipContext);

            // Assert
            Assert.That(isFeatureEnabled, Is.False);
        }

        [Test]
        public void ThenShouldReturnFalseIfFeatureIsDisabled()
        {
            // Arrange
            _featureToggleServiceMock.Setup(f => f.GetDataFromTableStorage())
                .Returns(new FeatureToggleConfiguration
                {
                    Data = new List<FeatureToggle>
                    {
                        new FeatureToggle
                        {
                            Controller = ControllerName,
                            Action = ActionName
                        }
                    }
                });

            // Act
            var isFeatureEnabled = _featureToggleService.IsFeatureEnabled(ControllerName, ActionName, _membershipContext);

            // Assert
            Assert.That(isFeatureEnabled, Is.False);
        }

        [Test]
        public void ThenShouldLogTrueResultIfFeatureIsEnabled()
        {
            // Act
            _featureToggleService.IsFeatureEnabled(ControllerName, ActionName, _membershipContext);

            // Assert
            _logger.Verify(l => l.Info($"Is feature enabled check for controllerName '{ControllerName}', actionName '{ActionName}' and userId '{_membershipContext.UserId}' is '{true}'."), Times.Once);
        }

        [Test]
        public void ThenShouldLogFalseResultIfFeatureIsDisabled()
        {
            // Arrange
            _featureToggleServiceMock.Setup(f => f.GetDataFromTableStorage())
                .Returns(new FeatureToggleConfiguration
                {
                    Data = new List<FeatureToggle>
                    {
                        new FeatureToggle
                        {
                            Controller = ControllerName,
                            Action = ActionName
                        }
                    }
                });

            // Act
            _featureToggleService.IsFeatureEnabled(ControllerName, ActionName, _membershipContext);

            // Assert
            _logger.Verify(l => l.Info($"Is feature enabled check for controllerName '{ControllerName}', actionName '{ActionName}' and userId '{_membershipContext.UserId}' is '{false}'."), Times.Once);
        }

        [Test]
        public void ThenShouldLogNullUserExternalIdIfMembershipIsNull()
        {
            // Act
            _featureToggleService.IsFeatureEnabled(ControllerName, ActionName, null);

            // Assert
            _logger.Verify(l => l.Info($"Is feature enabled check for controllerName '{ControllerName}', actionName '{ActionName}' and userId '{null}' is '{true}'."), Times.Once);
        }
    }
}