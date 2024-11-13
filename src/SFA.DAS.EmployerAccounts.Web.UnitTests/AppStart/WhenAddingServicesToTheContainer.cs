using System;
using System.Collections.Generic;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.AcceptInvitation;
using SFA.DAS.EmployerAccounts.Commands.AddPayeToAccount;
using SFA.DAS.EmployerAccounts.Commands.ChangeTeamMemberRole;
using SFA.DAS.EmployerAccounts.Commands.CreateAccount;
using SFA.DAS.EmployerAccounts.Commands.CreateAccountComplete;
using SFA.DAS.EmployerAccounts.Commands.CreateInvitation;
using SFA.DAS.EmployerAccounts.Commands.CreateLegalEntity;
using SFA.DAS.EmployerAccounts.Commands.CreateOrganisationAddress;
using SFA.DAS.EmployerAccounts.Commands.CreateUserAccount;
using SFA.DAS.EmployerAccounts.Commands.DeleteInvitation;
using SFA.DAS.EmployerAccounts.Commands.RemoveLegalEntity;
using SFA.DAS.EmployerAccounts.Commands.RemoveTeamMember;
using SFA.DAS.EmployerAccounts.Commands.RenameEmployerAccount;
using SFA.DAS.EmployerAccounts.Commands.ResendInvitation;
using SFA.DAS.EmployerAccounts.Commands.SignEmployerAgreement;
using SFA.DAS.EmployerAccounts.Commands.UnsubscribeNotification;
using SFA.DAS.EmployerAccounts.Commands.UnsubscribeProviderEmail;
using SFA.DAS.EmployerAccounts.Commands.UpdateOrganisationDetails;
using SFA.DAS.EmployerAccounts.Commands.UpdateShowWizard;
using SFA.DAS.EmployerAccounts.Commands.UpdateUserNotificationSettings;
using SFA.DAS.EmployerAccounts.Commands.UpsertRegisteredUser;
using SFA.DAS.EmployerAccounts.Queries.GetAccountEmployerAgreements;
using SFA.DAS.EmployerAccounts.Queries.GetAccountLegalEntities;
using SFA.DAS.EmployerAccounts.Queries.GetAccountLegalEntity;
using SFA.DAS.EmployerAccounts.Queries.GetAccountLegalEntityRemove;
using SFA.DAS.EmployerAccounts.Queries.GetAccountPayeSchemes;
using SFA.DAS.EmployerAccounts.Queries.GetAccountStats;
using SFA.DAS.EmployerAccounts.Queries.GetAccountTeamMembers;
using SFA.DAS.EmployerAccounts.Queries.GetApprenticeship;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAccount;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreement;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreementPdf;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerEnglishFractionHistory;
using SFA.DAS.EmployerAccounts.Queries.GetGatewayInformation;
using SFA.DAS.EmployerAccounts.Queries.GetGatewayToken;
using SFA.DAS.EmployerAccounts.Queries.GetHmrcEmployerInformation;
using SFA.DAS.EmployerAccounts.Queries.GetInvitation;
using SFA.DAS.EmployerAccounts.Queries.GetMember;
using SFA.DAS.EmployerAccounts.Queries.GetOrganisationAgreements;
using SFA.DAS.EmployerAccounts.Queries.GetOrganisationById;
using SFA.DAS.EmployerAccounts.Queries.GetOrganisations;
using SFA.DAS.EmployerAccounts.Queries.GetOrganisationsByAorn;
using SFA.DAS.EmployerAccounts.Queries.GetPayeSchemeByRef;
using SFA.DAS.EmployerAccounts.Queries.GetPensionRegulator;
using SFA.DAS.EmployerAccounts.Queries.GetProviderInvitation;
using SFA.DAS.EmployerAccounts.Queries.GetReservations;
using SFA.DAS.EmployerAccounts.Queries.GetSignedEmployerAgreementPdf;
using SFA.DAS.EmployerAccounts.Queries.GetSingleCohort;
using SFA.DAS.EmployerAccounts.Queries.GetTaskSummary;
using SFA.DAS.EmployerAccounts.Queries.GetTeamUser;
using SFA.DAS.EmployerAccounts.Queries.GetUnsignedEmployerAgreement;
using SFA.DAS.EmployerAccounts.Queries.GetUser;
using SFA.DAS.EmployerAccounts.Queries.GetUserAccountRole;
using SFA.DAS.EmployerAccounts.Queries.GetUserAccounts;
using SFA.DAS.EmployerAccounts.Queries.GetUserByRef;
using SFA.DAS.EmployerAccounts.Queries.GetUserInvitations;
using SFA.DAS.EmployerAccounts.Queries.GetUserNotificationSettings;
using SFA.DAS.EmployerAccounts.Queries.GetVacancies;
using SFA.DAS.EmployerAccounts.Queries.RemovePayeFromAccount;
using SFA.DAS.EmployerAccounts.Web.Orchestrators;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.AppStart;

public class WhenAddingServicesToTheContainer
{
    [TestCase(typeof(EmployerAccountOrchestrator))]
    [TestCase(typeof(EmployerAccountPayeOrchestrator))]
    [TestCase(typeof(EmployerAgreementOrchestrator))]
    [TestCase(typeof(EmployerTeamOrchestrator))]
    [TestCase(typeof(EmployerTeamOrchestratorWithCallToAction))]
    [TestCase(typeof(IHomeOrchestrator))]
    [TestCase(typeof(InvitationOrchestrator))]
    [TestCase(typeof(OrganisationOrchestrator))]
    [TestCase(typeof(SearchOrganisationOrchestrator))]
    [TestCase(typeof(SearchPensionRegulatorOrchestrator))]
    [TestCase(typeof(SupportErrorOrchestrator))]
    [TestCase(typeof(UserSettingsOrchestrator))]
    public void Then_The_Dependencies_Are_Correctly_Resolved_For_Orchestrators(Type toResolve)
    {
        RunTestForType(toResolve);
    }

    [TestCase(typeof(IRequestHandler<RenameEmployerAccountCommand>))]
    [TestCase(typeof(IRequestHandler<CreateLegalEntityCommand, CreateLegalEntityCommandResponse>))]
    [TestCase(typeof(IRequestHandler<AddPayeToAccountCommand>))]
    [TestCase(typeof(IRequestHandler<CreateAccountCommand, CreateAccountCommandResponse>))]
    [TestCase(typeof(IRequestHandler<CreateUserAccountCommand, CreateUserAccountCommandResponse>))]
    [TestCase(typeof(IRequestHandler<AddPayeToAccountCommand>))]
    [TestCase(typeof(IRequestHandler<RemovePayeFromAccountCommand>))]
    [TestCase(typeof(IRequestHandler<SignEmployerAgreementCommand, SignEmployerAgreementCommandResponse>))]
    [TestCase(typeof(IRequestHandler<RemoveLegalEntityCommand>))]
    [TestCase(typeof(IRequestHandler<DeleteInvitationCommand>))]
    [TestCase(typeof(IRequestHandler<ChangeTeamMemberRoleCommand>))]
    [TestCase(typeof(IRequestHandler<UpdateShowAccountWizardCommand>))]
    [TestCase(typeof(IRequestHandler<CreateInvitationCommand>))]
    [TestCase(typeof(IRequestHandler<RemoveTeamMemberCommand>))]
    [TestCase(typeof(IRequestHandler<ResendInvitationCommand>))]
    [TestCase(typeof(IRequestHandler<UnsubscribeProviderEmailCommand>))]
    [TestCase(typeof(IRequestHandler<UpsertRegisteredUserCommand>))]
    [TestCase(typeof(IRequestHandler<UpdateTermAndConditionsAcceptedOnCommand>))]
    [TestCase(typeof(IRequestHandler<AcceptInvitationCommand>))]
    [TestCase(typeof(IRequestHandler<UpdateOrganisationDetailsCommand>))]
    [TestCase(typeof(IRequestHandler<UpdateUserNotificationSettingsCommand>))]
    [TestCase(typeof(IRequestHandler<UnsubscribeNotificationCommand>))]
    [TestCase(typeof(IRequestHandler<SendAccountTaskListCompleteNotificationCommand>))]
    public void Then_The_Dependencies_Are_Correctly_Resolved_For_Command_Handlers(Type toResolve)
    {
        RunTestForType(toResolve);
    }

    [TestCase(typeof(IRequestHandler<GetEmployerAccountByIdQuery, GetEmployerAccountByIdResponse>))]
    [TestCase(typeof(IRequestHandler<GetUserAccountsQuery, GetUserAccountsQueryResponse>))]
    [TestCase(typeof(IRequestHandler<GetAccountPayeSchemesForAuthorisedUserQuery, GetAccountPayeSchemesResponse>))]
    [TestCase(typeof(IRequestHandler<GetMemberRequest, GetMemberResponse>))]
    [TestCase(typeof(IRequestHandler<GetEmployerAccountByIdQuery, GetEmployerAccountByIdResponse>))]
    [TestCase(typeof(IRequestHandler<GetPayeSchemeByRefQuery, GetPayeSchemeByRefResponse>))]
    [TestCase(typeof(IRequestHandler<GetEmployerEnglishFractionHistoryQuery, GetEmployerEnglishFractionHistoryResponse>))]
    [TestCase(typeof(IRequestHandler<GetTeamMemberQuery, GetTeamMemberResponse>))]
    [TestCase(typeof(IRequestHandler<GetAccountEmployerAgreementsRequest, GetAccountEmployerAgreementsResponse>))]
    [TestCase(typeof(IRequestHandler<GetEmployerAgreementRequest, GetEmployerAgreementResponse>))]
    [TestCase(typeof(IRequestHandler<GetNextUnsignedEmployerAgreementRequest, GetNextUnsignedEmployerAgreementResponse>))]
    [TestCase(typeof(IRequestHandler<GetEmployerAgreementPdfRequest, GetEmployerAgreementPdfResponse>))]
    [TestCase(typeof(IRequestHandler<GetSignedEmployerAgreementPdfRequest, GetSignedEmployerAgreementPdfResponse>))]
    [TestCase(typeof(IRequestHandler<GetAccountLegalEntityRemoveRequest, GetAccountLegalEntityRemoveResponse>))]
    [TestCase(typeof(IRequestHandler<GetOrganisationAgreementsRequest, GetOrganisationAgreementsResponse>))]
    [TestCase(typeof(IRequestHandler<GetEmployerAccountByIdQuery, GetEmployerAccountByIdResponse>))]
    [TestCase(typeof(IRequestHandler<GetTeamMemberQuery, GetTeamMemberResponse>))]
    [TestCase(typeof(IRequestHandler<GetAccountStatsQuery, GetAccountStatsResponse>))]
    [TestCase(typeof(IRequestHandler<GetAccountEmployerAgreementsRequest, GetAccountEmployerAgreementsResponse>))]
    [TestCase(typeof(IRequestHandler<GetUserByRefQuery, GetUserByRefResponse>))]
    [TestCase(typeof(IRequestHandler<GetTaskSummaryQuery, GetTaskSummaryResponse>))]
    [TestCase(typeof(IRequestHandler<GetInvitationRequest, GetInvitationResponse>))]
    [TestCase(typeof(IRequestHandler<GetAccountTeamMembersQuery, GetAccountTeamMembersResponse>))]
    [TestCase(typeof(IRequestHandler<GetUserQuery, GetUserResponse>))]
    [TestCase(typeof(IRequestHandler<GetEmployerAccountByIdQuery, GetEmployerAccountByIdResponse>))]
    [TestCase(typeof(IRequestHandler<GetReservationsRequest, GetReservationsResponse>))]
    [TestCase(typeof(IRequestHandler<GetApprenticeshipsRequest, GetApprenticeshipsResponse>))]
    [TestCase(typeof(IRequestHandler<GetSingleCohortRequest, GetSingleCohortResponse>))]
    [TestCase(typeof(IRequestHandler<GetVacanciesRequest, GetVacanciesResponse>))]
    [TestCase(typeof(IRequestHandler<GetUserAccountRoleQuery, GetUserAccountRoleResponse>))]
    [TestCase(typeof(IRequestHandler<GetGatewayInformationQuery, GetGatewayInformationResponse>))]
    [TestCase(typeof(IRequestHandler<GetGatewayTokenQuery, GetGatewayTokenQueryResponse>))]
    [TestCase(typeof(IRequestHandler<GetHmrcEmployerInformationQuery, GetHmrcEmployerInformationResponse>))]
    [TestCase(typeof(IRequestHandler<GetNumberOfUserInvitationsQuery, GetNumberOfUserInvitationsResponse>))]
    [TestCase(typeof(IRequestHandler<GetProviderInvitationQuery, GetProviderInvitationResponse>))]
    [TestCase(typeof(IRequestHandler<GetUserInvitationsRequest, GetUserInvitationsResponse>))]
    [TestCase(typeof(IRequestHandler<CreateOrganisationAddressRequest, CreateOrganisationAddressResponse>))]
    [TestCase(typeof(IRequestHandler<GetAccountLegalEntityRequest, GetAccountLegalEntityResponse>))]
    [TestCase(typeof(IRequestHandler<GetOrganisationByIdRequest, GetOrganisationByIdResponse>))]
    [TestCase(typeof(IRequestHandler<GetOrganisationsRequest, GetOrganisationsResponse>))]
    [TestCase(typeof(IRequestHandler<GetAccountLegalEntitiesRequest, GetAccountLegalEntitiesResponse>))]
    [TestCase(typeof(IRequestHandler<GetPensionRegulatorRequest, GetPensionRegulatorResponse>))]
    [TestCase(typeof(IRequestHandler<GetOrganisationsByAornRequest, GetOrganisationsByAornResponse>))]
    [TestCase(typeof(IRequestHandler<GetUserNotificationSettingsQuery, GetUserNotificationSettingsQueryResponse>))]
    public void Then_The_Dependencies_Are_Correctly_Resolved_For_Query_Handlers(Type toResolve)
    {
        RunTestForType(toResolve);
    }

    private static void RunTestForType(Type toResolve)
    {
        var mockHostingEnvironment = new Mock<IHostingEnvironment>();
        mockHostingEnvironment.Setup(x => x.EnvironmentName).Returns("Test");

        var startup = new Startup(GenerateStubConfiguration(), new Mock<IWebHostEnvironment>().Object, false);
        var serviceCollection = new ServiceCollection();
        startup.ConfigureServices(serviceCollection);

        serviceCollection.AddSingleton(_ => mockHostingEnvironment.Object);
        serviceCollection.AddSingleton(Mock.Of<IMessageSession>());
        var provider = serviceCollection.BuildServiceProvider();

        var type = provider.GetService(toResolve);
        Assert.That(type, Is.Not.Null);
    }

    private static ConfigurationRoot GenerateStubConfiguration()
    {
        var configSource = new MemoryConfigurationSource
        {
            InitialData = new List<KeyValuePair<string, string>>
            {
                new("SFA.DAS.Encoding",
                    "{\"Encodings\": [{\"EncodingType\": \"AccountId\",\"Salt\": \"and vinegar\",\"MinHashLength\": 32,\"Alphabet\": \"46789BCDFGHJKLMNPRSTVWXY\"}]}"),
                new("SFA.DAS.EmployerAccounts:DatabaseConnectionString",
                    "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true"),
                new("SFA.DAS.EmployerAccounts:AccountApiConfiguration:ApiBaseUrl", "https://localhost:1"),
                new("SFA.DAS.EmployerAccounts:EmployerAccountsConfiguration:OuterApiApiBaseUri", "https://localhost:1"),
                new("SFA.DAS.EmployerAccounts:EmployerAccountsConfiguration:OuterApiSubscriptionKey", "test"),
                new("SFA.DAS.EmployerAccounts:ContentApi:ApiBaseUrl", "test"),
                new("SFA.DAS.EmployerAccounts:ContentApi:IdentifierUrl", "test"),
                new("SFA.DAS.EmployerAccounts:ProviderRegistrationsApi:BaseUrl", "test"),
                new("SFA.DAS.EmployerAccounts:ProviderRegistrationsApi:IdentifierUrl", "test"),
                new("Environment", "test"),
                new("EnvironmentName", "test"),
                new("APPINSIGHTS_INSTRUMENTATIONKEY", "test"),
                new("SFA.DAS.EmployerAccounts:CommitmentsApi:IdentifierUrl", "test"),
                new("SFA.DAS.EmployerAccounts:CommitmentsApi:ApiBaseUrl", "test"),
                new("SFA.DAS.EmployerAccounts:DefaultServiceTimeoutMilliseconds", "100"),
                new("SFA.DAS.EmployerAccounts:EmployerAccountsOuterApiConfiguration:BaseUrl", "https://test.test"),
                new("SFA.DAS.EmployerAccounts:EmployerAccountsOuterApiConfiguration:Key", "test"),
                new("SFA.DAS.EmployerAccounts:Hmrc:BaseUrl", "https://test.test"),
                new("SFA.DAS.EmployerAccounts:PensionRegulatorApi:IdentifierUri", "test"),
                new("SFA.DAS.EmployerAccounts:PensionRegulatorApi:BaseUrl", "test"),
                new("SFA.DAS.EmployerAccounts:RecruitApi:IdentifierUri", "test"),
                new("SFA.DAS.EmployerAccounts:AuditApi:BaseUrl", "https://test.test"),
                new("SFA.DAS.EmployerAccounts:AuditApi:IdentifierUri", "test"),
                new("SFA.DAS.EmployerAccounts:EmployerAccountsOuterApiConfiguration:ApiBaseUrl", "https://test.test"),
                new("SFA.DAS.EmployerAccounts:EmployerAccountsOuterApiConfiguration:ClientId", "test"),
                new("SFA.DAS.EmployerAccounts:TokenServiceApi:ApiBaseUrl", "https://test.test"),
                new("SFA.DAS.EmployerAccounts:TokenServiceApi:ClientId", "test"),
                new("SFA.DAS.EmployerAccounts:TokenServiceApi:ClientSecret", "test"),
                new("SFA.DAS.EmployerAccounts:TokenServiceApi:IdentifierUrl", "https://test.test"),
                new("SFA.DAS.EmployerAccounts:TokenServiceApi:Tenant", "test"),
                new("SFA.DAS.EmployerAccounts:TasksApi:ApiBaseUrl", "https://test.test"),
                new("SFA.DAS.EmployerAccounts:TasksApi:IdentifierUrl", "https://test.test"),
                new("ResourceEnvironmentName", "TEST"),
                new("SFA.DAS.EmployerAccounts:Identity:ClientId", "clientId"),
                new("SFA.DAS.Employer.GovSignIn:GovUkOidcConfiguration:BaseUrl", "https://local.test.com"),
                new("SFA.DAS.Employer.GovSignIn:GovUkOidcConfiguration:ClientId", "test"),
                new("SFA.DAS.Employer.GovSignIn:GovUkOidcConfiguration:KeyVaultIdentifier", "https://local.test.com"),
                new("SFA.DAS.Employer.GovSignIn:GovUkOidcConfiguration:LoginSlidingExpiryTimeOutInMinutes", "30"),
                new("SFA.DAS.Employer.GovSignIn:GovUkOidcConfiguration:GovLoginSessionConnectionString", "https://local.test.com"),
            }
        };

        var provider = new MemoryConfigurationProvider(configSource);

        return new ConfigurationRoot(new List<IConfigurationProvider> { provider });
    }
}