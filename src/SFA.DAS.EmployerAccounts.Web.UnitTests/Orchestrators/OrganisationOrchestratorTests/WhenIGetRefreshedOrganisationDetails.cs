using AutoFixture;
using AutoMapper;
using MediatR;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Models.Organisation;
using SFA.DAS.EmployerAccounts.Queries.GetAccountLegalEntity;
using SFA.DAS.EmployerAccounts.Queries.GetOrganisationById;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Orchestrators.OrganisationOrchestratorTests;

[TestFixture]
public class WhenIGetRefreshedOrganisationDetails
{
    private Mock<IEncodingService> _encodingServiceMock;
    private Mock<IMediator> _mediatorMock;
    private Mock<IMapper> _mapper;
    private Mock<ICookieStorageService<EmployerAccountData>> _cookieService;
    private OrganisationOrchestrator _orchestrator;
    private Fixture _fixture;

    private readonly string hashedAccountLegalEntityId = "XXXX";
    private readonly int accountLegalEntityId = 1;
        
    [SetUp]
    public void SetUp()
    {
        _mapper = new Mock<IMapper>();
        _encodingServiceMock = new Mock<IEncodingService>();
        _mediatorMock = new Mock<IMediator>();
        _cookieService = new Mock<ICookieStorageService<EmployerAccountData>>();

        _fixture = new Fixture();

        _encodingServiceMock.Setup(x => x.Decode(hashedAccountLegalEntityId, EncodingType.PublicAccountLegalEntityId))
            .Returns(accountLegalEntityId);

        _orchestrator = new OrganisationOrchestrator(_mediatorMock.Object, _mapper.Object, _cookieService.Object, _encodingServiceMock.Object);
    }

    [Test]
    public async Task GetRefreshedOrganisationDetails_ShouldReturnValidResponse_WhenAddressIsNotNull()
    {
        // Arrange
        var currentDetails = _fixture.Build<GetAccountLegalEntityResponse>()
            .With(x => x.AccountLegalEntity, new AccountLegalEntityModel
            {
                Identifier = "123",
                OrganisationType = OrganisationType.CompaniesHouse,
                Name = "Test Organisation",
                Address = "123 Test St, AB12 3CD"
            })
            .Create();

        var refreshedDetails = _fixture.Build<GetOrganisationByIdResponse>()
            .With(x => x.Organisation, new ReferenceData.Types.DTO.Organisation
            {
                Name = "Updated Organisation",
                Address = new ReferenceData.Types.DTO.Address { Line1 = "123 New St", Postcode = "XY12 4ZQ" }
            })
            .Create();


        _mediatorMock.Setup(x => x.Send(It.IsAny<GetAccountLegalEntityRequest>(), default))
            .ReturnsAsync(currentDetails);

        _mediatorMock.Setup(x => x.Send(It.IsAny<GetOrganisationByIdRequest>(), default))
            .ReturnsAsync(refreshedDetails);

        // Act
        var result = await _orchestrator.GetRefreshedOrganisationDetails(hashedAccountLegalEntityId);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.OrganisationName.Should().Be("Test Organisation");
        result.Data.RefreshedName.Should().Be("Updated Organisation");
        result.Data.OrganisationAddress.Should().Be("123 Test St, AB12 3CD");
        result.Data.RefreshedAddress.Should().Be("123 New St, XY12 4ZQ");
        result.Data.UpdatesAvailable.Should().Be(OrganisationUpdatesAvailable.Any);
    }


    [Test]
    public async Task GetRefreshedOrganisationDetails_ShouldHandle_Both_NullAddressFields()
    {
        // Arrange
        var currentDetails = _fixture.Build<GetAccountLegalEntityResponse>()
            .With(x => x.AccountLegalEntity, new AccountLegalEntityModel
            {
                Identifier = "123",
                OrganisationType = OrganisationType.CompaniesHouse,
                Name = "Test Organisation",
                Address = null
            })
            .Create();

        // Refreshed details with null address fields

        var refreshedDetails = _fixture.Build<GetOrganisationByIdResponse>()
            .With(x => x.Organisation, new ReferenceData.Types.DTO.Organisation
            {
                Name = "Updated Organisation",
                Address = new ReferenceData.Types.DTO.Address 
                    { Line1 = null, Line2 = null, Line3 = null, Line4 = null, Line5 = null, Postcode = null }
            })
            .Create();

        _encodingServiceMock.Setup(x => x.Decode(hashedAccountLegalEntityId, EncodingType.PublicAccountLegalEntityId))
            .Returns(accountLegalEntityId);

        _mediatorMock.Setup(x => x.Send(It.IsAny<GetAccountLegalEntityRequest>(), default))
            .ReturnsAsync(currentDetails);

        _mediatorMock.Setup(x => x.Send(It.IsAny<GetOrganisationByIdRequest>(), default))
            .ReturnsAsync(refreshedDetails);

        // Act
        var result = await _orchestrator.GetRefreshedOrganisationDetails(hashedAccountLegalEntityId);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.OrganisationName.Should().Be("Test Organisation");
        result.Data.RefreshedName.Should().Be("Updated Organisation");
        result.Data.OrganisationAddress.Should().BeNull();
        result.Data.RefreshedAddress.Should().BeNull();
        result.Data.UpdatesAvailable.Should().Be(OrganisationUpdatesAvailable.Name);
    }
}