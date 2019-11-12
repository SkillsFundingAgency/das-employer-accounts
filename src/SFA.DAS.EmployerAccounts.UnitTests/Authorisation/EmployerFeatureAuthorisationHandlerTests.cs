﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Authorization.Context;
using SFA.DAS.Authorization.EmployerFeatures.Models;
using SFA.DAS.Authorization.Features.Models;
using SFA.DAS.Authorization.Features.Services;
using SFA.DAS.Authorization.Handlers;
using SFA.DAS.Authorization.Results;
using SFA.DAS.EmployerAccounts.AuthorisationExtensions;
using SFA.DAS.Testing;

namespace SFA.DAS.Authorization.Features.UnitTests.Handlers
{
    [TestFixture]
    [Parallelizable]
    public class EmployerFeatureAuthorisationHandlerTests : FluentTest<EmployerFeatureAuthorisationHandlerTestsFixture>
    {
        [Test]
        public Task GetAuthorizationResult_WhenOptionsAreNotAvailable_ThenShouldReturnValidAuthorizationResult()
        {
            //return TestAsync(f => f.GetAuthorizationResult(), (f, r) => r.Should().NotBeNull().And.Match<AuthorizationResult>(r2 => r2.IsAuthorized));
        }

        [Test]
        public Task GetAuthorizationResult_WhenAndedOptionsAreAvailable_ThenShouldThrowNotImplementedException()
        {
           //return TestExceptionAsync(f => f.SetAndedOptions(), f => f.GetAuthorizationResult(), (f, r) => r.Should().Throw<NotImplementedException>());
        }

        [Test]
        public Task GetAuthorizationResult_WhenOredOptionIsAvailable_ThenShouldThrowNotImplementedException()
        {
           // return TestExceptionAsync(f => f.SetOredOption(), f => f.GetAuthorizationResult(), (f, r) => r.Should().Throw<NotImplementedException>());
        }
    }

    public class EmployerFeatureAuthorisationHandlerTestsFixture
    {
        public List<string> Options { get; set; }
        public IAuthorizationContext AuthorizationContext { get; set; }
        public IAuthorizationHandler Handler { get; set; }
        public Mock<IFeatureTogglesService<EmployerFeatureToggle>> FeatureTogglesService { get; set; }
        public Mock<IMediator> Mediator { get; set; }

        public EmployerFeatureAuthorisationHandlerTestsFixture()
        {
            Options = new List<string>();
            AuthorizationContext = new AuthorizationContext();
            FeatureTogglesService = new Mock<IFeatureTogglesService<EmployerFeatureToggle>>();
            Mediator = new Mock<IMediator>();
            Handler = new EmployerFeatureAuthorizationHandler(FeatureTogglesService.Object, Mediator.Object);
        }

        public Task<AuthorizationResult> GetAuthorizationResult()
        {
            return Handler.GetAuthorizationResult(Options, AuthorizationContext);
        }

        public EmployerFeatureAuthorisationHandlerTestsFixture SetNonFeatureOptions()
        {
            Options.AddRange(new[] { "Foo", "Bar" });

            return this;
        }

        public EmployerFeatureAuthorisationHandlerTestsFixture SetAndedOptions()
        {
            Options.AddRange(new[] { "ProviderRelationships", "Tickles" });

            return this;
        }

        public EmployerFeatureAuthorisationHandlerTestsFixture SetOredOption()
        {
            Options.Add($"ProviderRelationships,ProviderRelationships");

            return this;
        }

        public EmployerFeatureAuthorisationHandlerTestsFixture SetOption()
        {
            Options.AddRange(new[] { "ProviderRelationships" });

            return this;
        }

        public EmployerFeatureAuthorisationHandlerTestsFixture SetFeatureToggle(bool isEnabled, bool? isAccountIdWhitelisted = null, bool? isUserEmailWhitelisted = null)
        {
            var option = Options.Single();

            FeatureTogglesService.Setup(s => s.GetFeatureToggle(option)).Returns(new FeatureToggle { Feature = "ProviderRelationships", IsEnabled = isEnabled });

            return this;
        }
    }
}