using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Api.Controllers;
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

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Controllers.LegalEntitiesControllerTests
{
    [TestFixture]
    public class WhenGettingAllByAccountId
    {
        [Test, RecursiveMoqAutoData]
        public async Task GetAllByAccountId_ReturnsOk_WhenLegalEntitiesExist(
        long accountId,
        int pageNumber,
        int pageSize,
        string sortColumn,
        bool isAscending,
        GetAllAccountLegalEntitiesByHashedAccountIdQueryResult mockResponse,
        [Frozen] Mock<IMediator> mediator,
        [Greedy] LegalEntitiesController controller,
        CancellationToken token)
        {
            // Arrange
            var pagedResult = new PaginatedList<AccountLegalEntity>(mockResponse.LegalEntities.Items.ToList(), 1, pageNumber, pageSize);
            mockResponse.LegalEntities = pagedResult;

            mediator.Setup(p => p.Send(It.Is<GetAllAccountLegalEntitiesByHashedAccountIdQuery>(q =>
                    q.AccountId == accountId &&
                    q.PageNumber == pageNumber && q.PageSize == pageSize && q.SortColumn == sortColumn && q.IsAscending == isAscending), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(mockResponse);

            // Act
            var result = await controller.GetAllLegalEntities(accountId, pageNumber, pageSize, sortColumn, isAscending, token);

            // Assert
            result.Should().BeOfType<Ok<GetAllAccountLegalEntitiesResponse>>();
            var okResult = result as Ok<GetAllAccountLegalEntitiesResponse>;

            okResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            okResult.Value!.LegalEntities.Count().Should().Be(mockResponse.LegalEntities.Items.Count);
            okResult.Value!.PageInfo.PageSize.Should().Be(pageSize);
            okResult.Value!.PageInfo.PageIndex.Should().Be(pageNumber);
        }

        [Test, RecursiveMoqAutoData]
        public async Task GetAllByAccountId_Returns_Empty_WhenNoEntitiesExist(
            long accountId,
            int pageNumber,
            int pageSize,
            string sortColumn,
            bool isAscending,
            GetAllAccountLegalEntitiesByHashedAccountIdQueryResult mockResponse,
            [Frozen] Mock<IMediator> mediator,
            [Greedy] LegalEntitiesController controller,
            CancellationToken token)
        {
            // Arrange
            var pagedResult = new PaginatedList<AccountLegalEntity>([], 0, pageNumber, pageSize);
            mockResponse.LegalEntities = pagedResult;

            mediator.Setup(p => p.Send(It.Is<GetAllAccountLegalEntitiesByHashedAccountIdQuery>(q =>
                    q.AccountId == accountId &&
                    q.PageNumber == pageNumber && q.PageSize == pageSize && q.SortColumn == sortColumn && q.IsAscending == isAscending), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(mockResponse);

            // Act
            var result = await controller.GetAllLegalEntities(accountId, pageNumber, pageSize, sortColumn, isAscending, token);

            // Assert
            result.Should().BeOfType<Ok<GetAllAccountLegalEntitiesResponse>>();
            var okResult = result as Ok<GetAllAccountLegalEntitiesResponse>;

            okResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            okResult.Value!.LegalEntities.Count().Should().Be(0);
            okResult.Value!.PageInfo.PageSize.Should().Be(pageSize);
            okResult.Value!.PageInfo.PageIndex.Should().Be(pageNumber);
        }

        [Test, RecursiveMoqAutoData]
        public async Task GetAllByAccountId_Returns_Exception_WhenInvalidAccountId(
            long accountId,
            int pageNumber,
            int pageSize,
            string sortColumn,
            bool isAscending,
            [Frozen] Mock<IMediator> mediator,
            [Greedy] LegalEntitiesController controller,
            CancellationToken token)
        {
            // Arrange
            mediator.Setup(p => p.Send(It.Is<GetAllAccountLegalEntitiesByHashedAccountIdQuery>(q =>
                        q.AccountId == accountId &&
                        q.PageNumber == pageNumber &&
                        q.PageSize == pageSize &&
                        q.SortColumn == sortColumn &&
                        q.IsAscending == isAscending),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await controller.GetAllLegalEntities(accountId, pageNumber, pageSize, sortColumn, isAscending, token);

            // Assert
            result.Should().BeOfType<ProblemHttpResult>();
        }
    }
}
