using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Queries.GetPayeAccountByRef;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetPayeAccountByRefTests;

public class WhenIGetAPayeAccount
{
    private Mock<IPayeRepository> _payeRepository;
    private GetPayeAccountByRefQuery Query { get; set; }

    private GetPayeAccountByRefHandler RequestHandler { get; set; }

    private const long AccountId = 1667;
    private const string Ref = "ABC/123";
    private readonly DateTime _addedDateTime = DateTime.UtcNow;
    private GetPayeAccountByRefResponse _expectedResponse;

    [SetUp]
    public void Arrange()
    {
        _expectedResponse = new GetPayeAccountByRefResponse
        {
            AccountId = AccountId,
            AddedDate = _addedDateTime,
            RemovedDate = null
        };

        _payeRepository = new Mock<IPayeRepository>();
        _payeRepository
            .Setup(x => x.GetPayeAccountByRef(Ref, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expectedResponse);

        Query = new GetPayeAccountByRefQuery
        {
            Ref = Ref
        };

        RequestHandler = new GetPayeAccountByRefHandler(_payeRepository.Object);
    }

    [Test]
    public async Task ThenTheRepositoryIsCalled()
    {
        //Act
        var cancellationToken = new CancellationToken();
        await RequestHandler.Handle(Query, cancellationToken);

        //Assert
        _payeRepository.Verify(x => x.GetPayeAccountByRef(Query.Ref, cancellationToken), Times.Once);
    }

    [Test]
    public async Task ThenTheExpectedResponseIsReturned()
    {
        //Act
        var cancellationToken = new CancellationToken();
        var response = await RequestHandler.Handle(Query, cancellationToken);

        //Assert
        response.Should().BeEquivalentTo(_expectedResponse);
    }
}