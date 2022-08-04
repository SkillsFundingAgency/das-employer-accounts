﻿using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http.Results;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EAS.Application.Queries.GetLevyDeclaration;
using SFA.DAS.EAS.TestCommon.ObjectMothers;

namespace SFA.DAS.EAS.Account.Api.UnitTests.Controllers.AccountLevyControllerTests
{
    [TestFixture]
    public class WhenIGetLevyForAnAccount : AccountLevyControllerTests
    {        
        [Test]
        public async Task ThenTheLevyIsReturned()
        {
            //Arrange
            var hashedAccountId = "ABC123";
            //var levyResponse = new GetLevyDeclarationResponse { Declarations = LevyDeclarationViewsObjectMother.Create(12334, "abc123") };
            //Mediator.Setup(x => x.SendAsync(It.Is<GetLevyDeclarationRequest>(q => q.HashedAccountId == hashedAccountId))).ReturnsAsync(levyResponse);
            var fixture = new Fixture();
            var apiResponse = new List<LevyDeclarationViewModel>()
            {
                fixture.Create<LevyDeclarationViewModel>(),
                fixture.Create<LevyDeclarationViewModel>()
            };
            apiResponse[0].HashedAccountId = hashedAccountId;
            //apiResponse[0].PayeSchemeReference = "123/abc123";
            apiResponse[1].HashedAccountId = hashedAccountId;
            FinanceApiService.Setup(x => x.GetLevyDeclarations(hashedAccountId)).ReturnsAsync(apiResponse);

            //Act
            var response = await Controller.Index(hashedAccountId);

            //Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkNegotiatedContentResult<AccountResourceList<LevyDeclarationViewModel>>>(response);
            var model = response as OkNegotiatedContentResult<AccountResourceList<LevyDeclarationViewModel>>;

            model?.Content.Should().NotBeNull();
            Assert.IsTrue(model?.Content.TrueForAll(x => x.HashedAccountId == hashedAccountId));            
            model?.Content.ShouldAllBeEquivalentTo(apiResponse, options => options.Excluding(x => x.HashedAccountId).Excluding(x => x.PayeSchemeReference));
            //TODO : check
            //Assert.IsTrue(model?.Content[0].PayeSchemeReference == levyResponse.Declarations[0].EmpRef);
        }

        [Test]
        public async Task AndTheAccountDoesNotExistThenItIsNotReturned()
        {
            var hashedAccountId = "ABC123";
            var levyResponse = new GetLevyDeclarationResponse { Declarations = null };

            Mediator.Setup(x => x.SendAsync(It.Is<GetLevyDeclarationRequest>(q => q.HashedAccountId == hashedAccountId))).ReturnsAsync(levyResponse);

            var response = await Controller.Index(hashedAccountId);

            Assert.IsNotNull(response);
            Assert.IsInstanceOf<NotFoundResult>(response);
        }
    }   
}
