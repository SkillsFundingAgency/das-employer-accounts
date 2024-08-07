﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Api.Mappings;
using SFA.DAS.EmployerAccounts.Exceptions;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Queries.GetAccountLegalEntitiesByHashedAccountId;
using SFA.DAS.EmployerAccounts.TestCommon.Extensions;
using SFA.DAS.Testing.AutoFixture;
using SFA.DAS.Validation;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Controllers.LegalEntitiesControllerTests;

[TestFixture]
public class WhenIGetLegalEntitiesForAnAccount : LegalEntitiesControllerTests
{
    private long _accountId;
    private GetAccountLegalEntitiesByHashedAccountIdResponse _response;

    [Test]
    public async Task ThenTheLegalEntitiesAreReturned()
    {
        _accountId = 5513;
        _response = new GetAccountLegalEntitiesByHashedAccountIdResponse
        {
            LegalEntities =
                new List<AccountLegalEntity>
                {
                    new AccountLegalEntity
                    {
                        Id = 1,
                        LegalEntityId = 5
                    },
                    new AccountLegalEntity
                    {
                        Id = 4,
                        LegalEntityId = 9
                    }
                }
        };

        Mediator.Setup(x =>
            x.Send(It.Is<GetAccountLegalEntitiesByHashedAccountIdRequest>(q => q.AccountId == _accountId),
                It.IsAny<CancellationToken>())).ReturnsAsync(_response);

        SetupUrlHelperForAccountLegalEntityOne();
        SetupUrlHelperForAccountLegalEntityTwo();

        var response = await Controller.GetLegalEntities(_accountId);

        Assert.That(response, Is.Not.Null);
        Assert.That(response, Is.InstanceOf<OkObjectResult>());
        var model = ((OkObjectResult)response).Value as Types.ResourceList;

        model.Should().NotBeNull();

        foreach (var legalEntity in _response.LegalEntities)
        {
            var matchedEntity = model.Single(x => x.Id == legalEntity.LegalEntityId.ToString());
            matchedEntity.Href.Should()
                .Be($"/api/accounts/{_accountId}/legalentities/{legalEntity.LegalEntityId}");
        }
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_If_Set_To_Include_Details_Then_AccountLegalEntity_List_Returned(
        List<AccountLegalEntity> legalEntities)
    {
        var expectedModel = legalEntities.Select(c => LegalEntityMapping.MapFromAccountLegalEntity(c, null, false))
            .ToList();
        _accountId = 661;
        _response = new GetAccountLegalEntitiesByHashedAccountIdResponse
        {
            LegalEntities = legalEntities
        };

        Mediator.Setup(x =>
            x.Send(It.Is<GetAccountLegalEntitiesByHashedAccountIdRequest>(q => q.AccountId == _accountId),
                It.IsAny<CancellationToken>())).ReturnsAsync(_response);

        SetupUrlHelperForAccountLegalEntityOne();
        SetupUrlHelperForAccountLegalEntityTwo();

        var response = await Controller.GetLegalEntities(_accountId, true);

        Assert.That(response, Is.Not.Null);
        Assert.That(response, Is.InstanceOf<OkObjectResult>());
        var model = ((OkObjectResult)response).Value as List<Types.LegalEntity>;
        model.Should().NotBeNull();
        model.Should().BeEquivalentTo(expectedModel);
    }

    [Test]
    public async Task AndTheAccountCannotBeDecodedThenItIsNotReturned()
    {
        Mediator.Setup(
                x => x.Send(
                    It.Is<GetAccountLegalEntitiesByHashedAccountIdRequest>(q => q.AccountId == _accountId),
                    It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidRequestException(new Dictionary<string, string>()));

        var response = await Controller.GetLegalEntities(_accountId);

        Assert.That(response, Is.Not.Null);
        Assert.That(response, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task AndTheAccountDoesNotExistThenItIsNotReturned()
    {
        Mediator.Setup(
                x => x.Send(
                    It.Is<GetAccountLegalEntitiesByHashedAccountIdRequest>(q => q.AccountId == _accountId),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new GetAccountLegalEntitiesByHashedAccountIdResponse
                {
                    LegalEntities = new List<AccountLegalEntity>(0)
                });

        var response = await Controller.GetLegalEntities(_accountId);

        Assert.That(response, Is.Not.Null);
        Assert.That(response, Is.InstanceOf<NotFoundResult>());
    }

    private void SetupUrlHelperForAccountLegalEntityOne()
    {
        UrlTestHelper.Setup(
                x => x.RouteUrl(
                    It.Is<UrlRouteContext>(c => c.RouteName == "GetLegalEntity" && c.Values.IsEquivalentTo(new
                    {
                        hashedAccountId =_accountId,
                        legalEntityId = _response.LegalEntities[0].LegalEntityId
                    }))))
            .Returns($"/api/accounts/{_accountId}/legalentities/{_response.LegalEntities[0].LegalEntityId}");
    }

    private void SetupUrlHelperForAccountLegalEntityTwo()
    {
        UrlTestHelper.Setup(
                x => x.RouteUrl(
                    It.Is<UrlRouteContext>(c => c.RouteName == "GetLegalEntity" && c.Values.IsEquivalentTo(new
                    {
                        hashedAccountId =_accountId,
                        legalEntityId = _response.LegalEntities[1].LegalEntityId
                    }))))
            .Returns($"/api/accounts/{_accountId}/legalentities/{_response.LegalEntities[1].LegalEntityId}");
    }
}