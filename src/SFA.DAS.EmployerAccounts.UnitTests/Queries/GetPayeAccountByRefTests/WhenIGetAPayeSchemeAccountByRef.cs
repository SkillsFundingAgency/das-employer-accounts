using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.PAYE;
using SFA.DAS.EmployerAccounts.Queries.GetPayeSchemeAccountByRef;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetPayeAccountByRefTests;

public class WhenIGetAPayeSchemeAccountByRef
{
    private Mock<IEmployerSchemesRepository> _employerSchemesRepository;
    private GetPayeSchemeAccountByRefQuery Query { get; set; }

    private GetPayeSchemeAccountByRefHandler RequestHandler { get; set; }

    private const long AccountId = 1667;
    private const string Ref = "ABC/123";
    private readonly DateTime _addedDateTime = DateTime.UtcNow;
    private PayeScheme _expectedResponse;

    [SetUp]
    public void Arrange()
    {
        _expectedResponse = new PayeScheme
        {
            AccountId = AccountId,
            AddedDate = _addedDateTime,
            RemovedDate = null
        };

        _employerSchemesRepository = new Mock<IEmployerSchemesRepository>();
        _employerSchemesRepository
            .Setup(x => x.GetSchemeByRef(Ref))
            .ReturnsAsync(_expectedResponse);

        Query = new GetPayeSchemeAccountByRefQuery
        {
            Ref = Ref
        };

        RequestHandler = new GetPayeSchemeAccountByRefHandler(_employerSchemesRepository.Object);
    }

    [Test]
    public async Task ThenTheRepositoryIsCalled()
    {
        //Act
        var cancellationToken = new CancellationToken();
        await RequestHandler.Handle(Query, cancellationToken);

        //Assert
        _employerSchemesRepository.Verify(x => x.GetSchemeByRef(Query.Ref), Times.Once);
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