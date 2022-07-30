﻿using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http.Results;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EAS.Application.Queries.GetLevyDeclarationsByAccountAndPeriod;
using SFA.DAS.EAS.TestCommon.ObjectMothers;

namespace SFA.DAS.EAS.Account.Api.UnitTests.Controllers.AccountLevyControllerTests
{
    [TestFixture]
    public class WhenIGetLevyForAnAccountAndPeriod : AccountLevyControllerTests
    {
        //TODO : change the Return Type to Account.Api.Types
        [Test]
        public async Task ThenTheLevyIsReturned()
        {
            var hashedAccountId = "ABC123";
            var payrollYear = "2017-18";
            short payrollMonth = 5;
            var levyResponse = new GetLevyDeclarationsByAccountAndPeriodResponse { Declarations = LevyDeclarationViewsObjectMother.Create(12334, "abc123") };
            Mediator.Setup(
                    x => x.SendAsync(It.Is<GetLevyDeclarationsByAccountAndPeriodRequest>(q => q.HashedAccountId == hashedAccountId && q.PayrollYear == payrollYear && q.PayrollMonth == payrollMonth)))
                .ReturnsAsync(levyResponse);

            
            var fixture = new Fixture();
            ICollection<SFA.DAS.EAS.Finance.Api.Types.LevyDeclarationViewModel> apiResponse = new List<SFA.DAS.EAS.Finance.Api.Types.LevyDeclarationViewModel>()
            {
                 fixture.Create<SFA.DAS.EAS.Finance.Api.Types.LevyDeclarationViewModel>(),
                fixture.Create<SFA.DAS.EAS.Finance.Api.Types.LevyDeclarationViewModel>()
            };
            FinanceApiService.Setup(x => x.GetLevyForPeriod(hashedAccountId, payrollYear, payrollMonth)).ReturnsAsync(apiResponse);


            var response = await Controller.GetLevy(hashedAccountId, payrollYear, payrollMonth);

            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkNegotiatedContentResult<AccountResourceList<SFA.DAS.EAS.Account.Api.Types.LevyDeclarationViewModel>>>(response);
            var model = response as OkNegotiatedContentResult<AccountResourceList<SFA.DAS.EAS.Account.Api.Types.LevyDeclarationViewModel>>;

            model?.Content.Should().NotBeNull();
            Assert.IsTrue(model?.Content.TrueForAll(x => x.HashedAccountId == hashedAccountId));
            //model?.Content.ShouldAllBeEquivalentTo(levyResponse.Declarations, options => options.Excluding(x => x.HashedAccountId).Excluding(x => x.PayeSchemeReference));
            //Assert.IsTrue(model?.Content[0].PayeSchemeReference == levyResponse.Declarations[0].EmpRef);
        }

        [Test]
        public async Task AndTheAccountDoesNotExistThenItIsNotReturned()
        {
            var hashedAccountId = "ABC123";
            var levyResponse = new GetLevyDeclarationsByAccountAndPeriodResponse { Declarations = null };

            Mediator.Setup(x => x.SendAsync(It.Is<GetLevyDeclarationsByAccountAndPeriodRequest>(q => q.HashedAccountId == hashedAccountId))).ReturnsAsync(levyResponse);

            var response = await Controller.GetLevy(hashedAccountId, "2017-18", 6);

            Assert.IsNotNull(response);
            Assert.IsInstanceOf<NotFoundResult>(response);
        }
    }
}
