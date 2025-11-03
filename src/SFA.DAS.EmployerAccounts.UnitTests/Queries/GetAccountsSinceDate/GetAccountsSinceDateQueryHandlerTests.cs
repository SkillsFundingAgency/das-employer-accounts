using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Queries.GetAccountsSinceDate;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetAccountsSinceDate;

public class GetAccountsSinceDateQueryHandlerTests
    : QueryBaseTest<GetAccountsSinceDateQueryHandler, GetAccountsSinceDateQuery, GetAccountsSinceDateResponse>
{
    private Mock<IEmployerAccountRepository> _employerAccountRepository;
    public override GetAccountsSinceDateQuery Query { get; set; }
    public override GetAccountsSinceDateQueryHandler RequestHandler { get; set; }
    public override Mock<IValidator<GetAccountsSinceDateQuery>> RequestValidator { get; set; }

    private readonly DateTime SinceDate = DateTime.MinValue;
    private const int PageNumber = 1;
    private const int PageSize = 1000;

    [SetUp]
    public void Arrange()
    {
        SetUp();

        _employerAccountRepository = new Mock<IEmployerAccountRepository>();
        _employerAccountRepository
            .Setup(x => x.GetAccounts(SinceDate, PageNumber, PageSize))
            .ReturnsAsync(new Accounts<AccountNameSummary>
            {
                AccountList = new List<AccountNameSummary>
                {
                    new AccountNameSummary { Id = 1, Name = "Test1" },
                    new AccountNameSummary { Id = 2, Name = "Test2" }
                }
            });

        RequestHandler = new GetAccountsSinceDateQueryHandler(
            _employerAccountRepository.Object,
            RequestValidator.Object);

        Query = new GetAccountsSinceDateQuery
        {
            SinceDate = SinceDate,
            PageNumber = PageNumber,
            PageSize = PageSize
        };
    }

    [Test]
    public override async Task ThenIfTheMessageIsValidTheRepositoryIsCalled()
    {
        // Act
        await RequestHandler.Handle(Query, CancellationToken.None);

        // Assert
        _employerAccountRepository.Verify(
            x => x.GetAccounts(SinceDate, PageNumber, PageSize),
            Times.Once);
    }

    [Test]
    public override async Task ThenIfTheMessageIsValidTheValueIsReturnedInTheResponse()
    {
        // Act
        var result = await RequestHandler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Accounts.Should().NotBeNull();
        result.Accounts.AccountList.Should().NotBeNullOrEmpty();
        result.Accounts.AccountList.Should().HaveCount(2);
        result.Accounts.AccountList.Should()
            .ContainEquivalentOf(new AccountNameSummary { Id = 1, Name = "Test1" })
            .And.ContainEquivalentOf(new AccountNameSummary { Id = 2, Name = "Test2" });
    }
}
