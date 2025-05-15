using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Queries.GetAllAccountLegalEntitiesByHashedAccountId;
using SFA.DAS.Testing.AutoFixture;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetAllAccountLegalEntitiesByHashedAccountId
{
    [TestFixture]
    public class WhenHandlingGetAllAccountLegalEntitiesByHashedAccountIdQuery
    {
        [Test, RecursiveMoqAutoData]
        public async Task GetAllAccountLegalEntitiesByHashedAccountId_ShouldReturnPaginatedList_WhenCalledWithValidParameters(
            long accountId,
            int pageNumber,
            int pageSize,
            string sortColumn,
            bool isAscending,
            CancellationToken token,
            List<AccountLegalEntity> entities,
            [Frozen] Mock<IAccountLegalEntityRepository> repositoryMock,
            [Greedy] GetAllAccountLegalEntitiesByHashedAccountIdQueryHandler handler)
        {
            // Arrange
            sortColumn = "Name";
            isAscending = false;

            var expectedList = new PaginatedList<AccountLegalEntity>(entities, 10, pageNumber, pageSize);
            repositoryMock.Setup(repo => repo.GetAccountLegalEntities(accountId, pageNumber, pageSize, sortColumn, isAscending, token))
                .ReturnsAsync(expectedList);

            // Act
            var result = await handler.Handle(new GetAllAccountLegalEntitiesByHashedAccountIdQuery(accountId, pageNumber, pageSize, sortColumn, isAscending), token);

            // Assert
            result.LegalEntities.PageIndex.Should().Be(pageNumber);
            result.LegalEntities.PageSize.Should().Be(pageSize);
            result.LegalEntities.TotalCount.Should().Be(10);
            result.LegalEntities.Items.Should().BeEquivalentTo(entities);
            repositoryMock.Verify(repo => repo.GetAccountLegalEntities(accountId, pageNumber, pageSize, sortColumn, isAscending, token), Times.Once);
        }

        [Test, RecursiveMoqAutoData]
        public void GetAllAccountLegalEntitiesByHashedAccountId_ShouldThrowException_WhenRepositoryThrowsException(
            long accountId,
            int pageNumber,
            int pageSize,
            string sortColumn,
            bool isAscending,
            CancellationToken token,
            List<AccountLegalEntity> entities,
            [Frozen] Mock<IAccountLegalEntityRepository> repositoryMock,
            [Greedy] GetAllAccountLegalEntitiesByHashedAccountIdQueryHandler handler)
        {
            // Arrange
            sortColumn = "Name";
            isAscending = false;

            repositoryMock.Setup(repo => repo.GetAccountLegalEntities(accountId, pageNumber, pageSize, sortColumn, isAscending, token))
                .ThrowsAsync(new Exception("Repository exception"));

            // Act & Assert
            Assert.ThrowsAsync<Exception>(() => handler.Handle(new GetAllAccountLegalEntitiesByHashedAccountIdQuery(accountId, pageNumber, pageSize, sortColumn, isAscending), token));

            repositoryMock.Verify(repo => repo.GetAccountLegalEntities(accountId, pageNumber, pageSize, sortColumn, isAscending, token), Times.Once);
        }
    }
}
