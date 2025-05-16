using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Api.Controllers;
using SFA.DAS.EmployerAccounts.Api.Requests;
using SFA.DAS.EmployerAccounts.Api.Responses;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Queries.GetAllAccountLegalEntitiesByHashedAccountId;
using SFA.DAS.Testing.AutoFixture;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Controllers.AccountLegalEntitiesControllerTests
{
    [TestFixture]
    public class WhenGettingAllByAccountId
    {
        [Test, RecursiveMoqAutoData]
        public async Task GetAllByAccountId_ReturnsOk_WhenLegalEntitiesExist(
            GetAllLegalEntitiesRequest request,
            GetAllAccountLegalEntitiesByHashedAccountIdQueryResult mockResponse,
            [Frozen] Mock<IMediator> mediator,
            [Greedy] AccountLegalEntitiesController controller,
            CancellationToken token)
        {
            // Arrange
            var pagedResult = new PaginatedList<AccountLegalEntity>(mockResponse.LegalEntities.Items.ToList(), 1, request.PageNumber, request.PageSize);
            mockResponse.LegalEntities = pagedResult;

            mediator.Setup(p => p.Send(It.Is<GetAllAccountLegalEntitiesByHashedAccountIdQuery>(q =>
                    q.SearchTerm == request.SearchTerm &&
                    q.AccountIds == request.AccountIds &&
                    q.PageNumber == request.PageNumber &&
                    q.PageSize == request.PageSize &&
                    q.SortColumn == request.SortColumn &&
                    q.IsAscending == request.IsAscending), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(mockResponse);

            // Act
            var result = await controller.GetAllLegalEntities(request, token);

            // Assert
            result.Should().BeOfType<Ok<GetAllAccountLegalEntitiesResponse>>();
            var okResult = result as Ok<GetAllAccountLegalEntitiesResponse>;

            okResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            okResult.Value!.LegalEntities.Count().Should().Be(mockResponse.LegalEntities.Items.Count);
            okResult.Value!.PageInfo.PageSize.Should().Be(request.PageSize);
            okResult.Value!.PageInfo.PageIndex.Should().Be(request.PageNumber);
        }

        [Test, RecursiveMoqAutoData]
        public async Task GetAllByAccountId_Returns_Empty_WhenNoEntitiesExist(
            GetAllLegalEntitiesRequest request,
            GetAllAccountLegalEntitiesByHashedAccountIdQueryResult mockResponse,
            [Frozen] Mock<IMediator> mediator,
            [Greedy] AccountLegalEntitiesController controller,
            CancellationToken token)
        {
            // Arrange
            var pagedResult = new PaginatedList<AccountLegalEntity>([], 0, request.PageNumber, request.PageSize);
            mockResponse.LegalEntities = pagedResult;

            mediator.Setup(p => p.Send(It.Is<GetAllAccountLegalEntitiesByHashedAccountIdQuery>(q =>
                    q.SearchTerm == request.SearchTerm &&
                    q.AccountIds == request.AccountIds &&
                    q.PageNumber == request.PageNumber &&
                    q.PageSize == request.PageSize &&
                    q.SortColumn == request.SortColumn &&
                    q.IsAscending == request.IsAscending), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(mockResponse);

            // Act
            var result = await controller.GetAllLegalEntities(request, token);

            // Assert
            result.Should().BeOfType<Ok<GetAllAccountLegalEntitiesResponse>>();
            var okResult = result as Ok<GetAllAccountLegalEntitiesResponse>;

            okResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            okResult.Value!.LegalEntities.Count().Should().Be(0);
            okResult.Value!.PageInfo.PageSize.Should().Be(request.PageSize);
            okResult.Value!.PageInfo.PageIndex.Should().Be(request.PageNumber);
        }

        [Test, RecursiveMoqAutoData]
        public async Task GetAllByAccountId_Returns_Exception_WhenInvalidAccountId(
            GetAllLegalEntitiesRequest request,
            GetAllAccountLegalEntitiesByHashedAccountIdQueryResult mockResponse,
            [Frozen] Mock<IMediator> mediator,
            [Greedy] AccountLegalEntitiesController controller,
            CancellationToken token)
        {
            // Arrange
            mediator.Setup(p => p.Send(It.Is<GetAllAccountLegalEntitiesByHashedAccountIdQuery>(q =>
                    q.SearchTerm == request.SearchTerm &&
                    q.AccountIds == request.AccountIds &&
                    q.PageNumber == request.PageNumber &&
                    q.PageSize == request.PageSize &&
                    q.SortColumn == request.SortColumn &&
                    q.IsAscending == request.IsAscending), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await controller.GetAllLegalEntities(request, token);

            // Assert
            result.Should().BeOfType<ProblemHttpResult>();
        }
    }
}
