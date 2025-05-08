using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Data;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Queries.SearchEmployerAccountsByName;
using SFA.DAS.EmployerAccounts.TestCommon.DatabaseMock;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.SearchEmployerAccountsByNameTests;

[TestFixture]
public class WhenSearchingEmployerAccountsByName
{
    private Mock<EmployerAccountsDbContext> _dbContext;
    private Mock<IValidator<SearchEmployerAccountsByNameQuery>> _validator;
    private SearchEmployerAccountsByNameQuery _query;
    private List<Account> _accounts;
    private SearchEmployerAccountsByNameQueryHandler _handler;
    private Lazy<EmployerAccountsDbContext> _lazyDbContext;

    [SetUp]
    public void Arrange()
    {
        _dbContext = new Mock<EmployerAccountsDbContext>();
        _lazyDbContext = new Lazy<EmployerAccountsDbContext>(() => _dbContext.Object);
        _validator = new Mock<IValidator<SearchEmployerAccountsByNameQuery>>();
        _query = new SearchEmployerAccountsByNameQuery { EmployerName = "Test Account" };

        _accounts =
        [
            new Account { Id = 1, Name = "Test Account 1", HashedId = "ABC123", PublicHashedId = "PUB123" },
            new Account { Id = 2, Name = "Test Account 2", HashedId = "DEF456", PublicHashedId = "PUB456" }
        ];

        var mockDbSet = _accounts.AsQueryable().BuildMockDbSet();
        _dbContext.Setup(x => x.Accounts).Returns(mockDbSet.Object);

        _handler = new SearchEmployerAccountsByNameQueryHandler(_lazyDbContext, _validator.Object);
    }

    [Test]
    public async Task Then_If_The_Message_Is_Valid_The_Repository_Is_Called()
    {
        //Arrange
        _validator.Setup(x => x.Validate(It.IsAny<SearchEmployerAccountsByNameQuery>()))
            .Returns(new ValidationResult());

        //Act
        await _handler.Handle(_query, CancellationToken.None);

        //Assert
        _dbContext.Verify(x => x.Accounts, Times.Once);
    }

    [Test]
    public async Task Then_The_Query_Returns_The_Expected_Results()
    {
        //Arrange
        _validator.Setup(x => x.Validate(It.IsAny<SearchEmployerAccountsByNameQuery>()))
            .Returns(new ValidationResult());

        //Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        //Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(new List<EmployerAccountByNameResult>
        {
            new()
            {
                AccountId = 1,
                DasAccountName = "Test Account 1",
                HashedAccountId = "ABC123",
                PublicHashedAccountId = "PUB123"
            },
            new()
            {
                AccountId = 2,
                DasAccountName = "Test Account 2",
                HashedAccountId = "DEF456",
                PublicHashedAccountId = "PUB456"
            }
        });
    }

    [Test]
    public async Task Then_If_The_Message_Is_Invalid_An_Exception_Is_Thrown()
    {
        //Arrange
        _validator.Setup(x => x.Validate(It.IsAny<SearchEmployerAccountsByNameQuery>()))
            .Returns(new ValidationResult { ValidationDictionary = { { "EmployerName", "Employer name is required" } } });

        //Act
        var act = () => _handler.Handle(_query, CancellationToken.None);

        //Assert
        await act.Should().ThrowAsync<InvalidRequestException>();
    }

    [Test]
    public async Task Then_If_The_EmployerName_Is_Empty_An_Empty_List_Is_Returned()
    {
        //Arrange
        _validator.Setup(x => x.Validate(It.IsAny<SearchEmployerAccountsByNameQuery>()))
            .Returns(new ValidationResult());
        _query.EmployerName = string.Empty;

        //Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task Then_If_The_EmployerName_Is_Null_An_Empty_List_Is_Returned()
    {
        //Arrange
        _validator.Setup(x => x.Validate(It.IsAny<SearchEmployerAccountsByNameQuery>()))
            .Returns(new ValidationResult());
        _query.EmployerName = null;

        //Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
} 