using System;
using System.Collections.Generic;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Api.Orchestrators;
using SFA.DAS.EmployerAccounts.Api.ServiceRegistrations;
using SFA.DAS.EmployerAccounts.Commands.SupportChangeTeamMemberRole;
using SFA.DAS.EmployerAccounts.Commands.SupportResendInvitationCommand;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Factories;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Queries.GetAccountLegalEntitiesByHashedAccountId;
using SFA.DAS.EmployerAccounts.Queries.GetAccountPayeSchemes;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAccountDetail;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreementById;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreementsByAccountId;
using SFA.DAS.EmployerAccounts.Queries.GetMinimumSignedAgreementVersion;
using SFA.DAS.EmployerAccounts.Queries.GetPagedEmployerAccounts;
using SFA.DAS.EmployerAccounts.Queries.GetPayeSchemeByRef;
using SFA.DAS.EmployerAccounts.Queries.GetTeamMembers;
using SFA.DAS.EmployerAccounts.Queries.GetTeamMembersWhichReceiveNotifications;
using SFA.DAS.EmployerAccounts.Queries.GetUserAccounts;
using SFA.DAS.EmployerAccounts.Queries.GetUserByEmail;
using SFA.DAS.EmployerAccounts.Queries.RemovePayeFromAccount;
using SFA.DAS.EmployerAccounts.Queries.UpdateUserAornLock;
using SFA.DAS.EmployerAccounts.ServiceRegistration;
using SFA.DAS.Encoding;
using SFA.DAS.Notifications.Api.Client;
using SFA.DAS.Notifications.Api.Client.Configuration;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests;

public class WhenAddingServicesToTheContainer
{
    private static void RunTestForType(Type toResolve)
    {
        var serviceCollection = BuildServiceCollection();
        var provider = serviceCollection.BuildServiceProvider();

        var type = provider.GetService(toResolve);

        type.Should().NotBeNull();
    }
    
    [TestCase(typeof(AccountsOrchestrator))]
    [TestCase(typeof(AgreementOrchestrator))]
    [TestCase(typeof(UsersOrchestrator))]
    public void Then_The_Dependencies_Are_Correctly_Resolved_For_Orchestrators(Type toResolve)
    {
       RunTestForType(toResolve);
    }

    [TestCase(typeof(IRequestHandler<GetPayeSchemeByRefQuery, GetPayeSchemeByRefResponse>))]
    [TestCase(typeof(IRequestHandler<GetEmployerAccountDetailByHashedIdQuery, GetEmployerAccountDetailByHashedIdResponse>))]
    [TestCase(typeof(IRequestHandler<GetPagedEmployerAccountsQuery, GetPagedEmployerAccountsResponse>))]
    [TestCase(typeof(IRequestHandler<GetTeamMembersRequest, GetTeamMembersResponse>))]
    [TestCase(typeof(IRequestHandler<GetTeamMembersWhichReceiveNotificationsQuery, GetTeamMembersWhichReceiveNotificationsQueryResponse>))]
    [TestCase(typeof(IRequestHandler<GetAccountPayeSchemesQuery, GetAccountPayeSchemesResponse>))]
    [TestCase(typeof(IRequestHandler<GetUserAccountsQuery, GetUserAccountsQueryResponse>))]
    [TestCase(typeof(IRequestHandler<GetAccountPayeSchemesQuery, GetAccountPayeSchemesResponse>))]
    [TestCase(typeof(IRequestHandler<GetEmployerAgreementByIdRequest, GetEmployerAgreementByIdResponse>))]
    [TestCase(typeof(IRequestHandler<GetMinimumSignedAgreementVersionQuery, GetMinimumSignedAgreementVersionResponse>))]
    [TestCase(typeof(IRequestHandler<GetUserAccountsQuery, GetUserAccountsQueryResponse>))]
    [TestCase(typeof(IRequestHandler<GetAccountLegalEntitiesByHashedAccountIdRequest, GetAccountLegalEntitiesByHashedAccountIdResponse>))]
    [TestCase(typeof(IRequestHandler<GetEmployerAgreementsByAccountIdRequest, GetEmployerAgreementsByAccountIdResponse>))]
    [TestCase(typeof(IRequestHandler<GetUserByEmailQuery, GetUserByEmailResponse>))]
    [TestCase(typeof(IRequestHandler<UpdateUserAornLockRequest>))]
    [TestCase(typeof(IRequestHandler<RemovePayeFromAccountCommand>))]
    [TestCase(typeof(IRequestHandler<SupportResendInvitationCommand>))]
    [TestCase(typeof(IRequestHandler<SupportChangeTeamMemberRoleCommand>))]
    public void Then_The_Dependencies_Are_Correctly_Resolved_For_Handlers(Type toResolve)
    {
        RunTestForType(toResolve);
    }

    [TestCase(typeof(INotificationsApi))]
    public void Then_The_Dependencies_Are_Correctly_Resolved_For_Apis(Type toResolve)
    {
        RunTestForType(toResolve);
    }

    private static ServiceCollection BuildServiceCollection()
    {
        var mockHostingEnvironment = new Mock<IWebHostEnvironment>();
        mockHostingEnvironment.Setup(x => x.EnvironmentName).Returns("Test");

        var config = GenerateConfiguration();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton((IConfiguration)config);
        serviceCollection.AddSingleton(mockHostingEnvironment.Object);
        serviceCollection.AddSingleton(Mock.Of<IPayeSchemesService>());
        serviceCollection.AddSingleton(Mock.Of<IUserAornPayeLockService>());
        serviceCollection.AddSingleton(Mock.Of<IGenericEventFactory>());
        serviceCollection.AddSingleton(Mock.Of<IPayeSchemeEventFactory>());
        serviceCollection.AddSingleton(Mock.Of<IEventPublisher>());
        serviceCollection.AddSingleton(Mock.Of<IMessageSession>());
        serviceCollection.AddSingleton((IConfiguration)config);
        serviceCollection.AddHttpContextAccessor();
        serviceCollection.AddDatabaseRegistration();
        serviceCollection.AddDataRepositories();
        serviceCollection.AddOrchestrators();
        serviceCollection.AddAuditServices();
        serviceCollection.AddAutoMapper(typeof(Startup).Assembly);
        serviceCollection.AddApplicationServices();
        serviceCollection.AddApiConfigurationSections(config);
        serviceCollection.AddNotifications(config);
        serviceCollection.AddMediatR(serviceConfiguration => serviceConfiguration.RegisterServicesFromAssembly(typeof(GetAccountPayeSchemesQuery).Assembly));
        serviceCollection.AddMediatorValidators();
        serviceCollection.AddLogging();
        return serviceCollection;
    }

    private static IConfigurationRoot GenerateConfiguration()
    {
    var configSource = new MemoryConfigurationSource
    {
        InitialData = new List<KeyValuePair<string, string>>
            {
                new($"{ConfigurationKeys.EncodingConfig}", "{\"Encodings\": [{\"EncodingType\": \"AccountId\",\"Salt\": \"and vinegar\",\"MinHashLength\": 32,\"Alphabet\": \"46789BCDFGHJKLMNPRSTVWXY\"}]}"),
                    
                new($"{ConfigurationKeys.EmployerAccounts}:DatabaseConnectionString", "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;"),
                new($"{ConfigurationKeys.EmployerAccounts}:AccountApiConfiguration:ApiBaseUrl", "https://localhost:1"),
                new($"{ConfigurationKeys.EmployerAccounts}:OuterApiApiBaseUri", "https://localhost:1"),
                new($"{ConfigurationKeys.EmployerAccounts}:OuterApiSubscriptionKey", "test"),
                new($"{ConfigurationKeys.EmployerAccounts}:ContentApi:ApiBaseUrl", "test"),
                new($"{ConfigurationKeys.EmployerAccounts}:ContentApi:IdentifierUrl", "test"),
                new($"{ConfigurationKeys.EmployerAccounts}:ProviderRegistrationsApi:BaseUrl", "test"),
                new($"{ConfigurationKeys.EmployerAccounts}:ProviderRegistrationsApi:IdentifierUrl", "test"),
                new($"{ConfigurationKeys.EmployerAccounts}:Environment", "test"),
                new($"{ConfigurationKeys.EmployerAccounts}:EnvironmentName", "test"),
                new($"{ConfigurationKeys.EmployerAccounts}:APPINSIGHTS_INSTRUMENTATIONKEY", "test"),
                new($"{ConfigurationKeys.EmployerAccounts}:ElasticUrl", "test"),
                new($"{ConfigurationKeys.EmployerAccounts}:ElasticUsername", "test"),
                new($"{ConfigurationKeys.EmployerAccounts}:ElasticPassword", "test"),
                
                new($"{ConfigurationKeys.NotificationsApiClient}:ApiBaseUrl", "https://test.test/"),
                new($"{ConfigurationKeys.NotificationsApiClient}:ClientToken", "ABVCJKDS"),
            }
        };

        var provider = new MemoryConfigurationProvider(configSource);

        return new ConfigurationRoot(new List<IConfigurationProvider> { provider });
    }
}