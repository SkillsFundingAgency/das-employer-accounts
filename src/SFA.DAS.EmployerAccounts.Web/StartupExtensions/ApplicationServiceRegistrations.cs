﻿using SFA.DAS.Api.Common.Infrastructure;
using SFA.DAS.Api.Common.Interfaces;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmployerAccounts.Data;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Factories;
using SFA.DAS.EmployerAccounts.Policies.Hmrc;
using SFA.DAS.EmployerAccounts.Services;
using SFA.DAS.Encoding;
using SFA.DAS.GovUK.Auth.Services;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.Web.StartupExtensions;

public static class ApplicationServiceRegistrations
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IAzureClientCredentialHelper, AzureClientCredentialHelper>();

        services.AddSingleton<IPdfService, PdfService>();

        services.AddScoped<IHtmlHelpers, HtmlHelpers>();
        services.AddTransient<IHttpServiceFactory, HttpServiceFactory>();
        services.AddTransient<IUserAornPayeLockService, UserAornPayeLockService>();

        services.AddTransient<IEmployerAccountService, EmployerAccountService>();
        
        services.AddScoped<IReferenceDataService, ReferenceDataService>();

        services.AddTransient<IReservationsService, ReservationsService>();
        services.Decorate<IReservationsService, ReservationsServiceWithTimeout>();

        services.AddTransient<ICommitmentV2Service, CommitmentsV2Service>();
        services.Decorate<ICommitmentV2Service, CommitmentsV2ServiceWithTimeout>();

        services.AddTransient<IRecruitService, RecruitService>();
        services.Decorate<IRecruitService, RecruitServiceWithTimeout>();

        services.AddScoped<IAccountApiClient, AccountApiClient>();

        services.AddTransient<IPensionRegulatorService, PensionRegulatorService>();

        services.AddTransient<IDateTimeService, DateTimeService>();
        services.AddSingleton<IEncodingService, EncodingService>();

        services.AddTransient<IUserAccountRepository, UserAccountRepository>();

        services.AddScoped(typeof(ICookieService<>), typeof(HttpCookieService<>));
        services.AddScoped(typeof(ICookieStorageService<>), typeof(CookieStorageService<>));
        services.AddSingleton<IUrlActionHelper, UrlActionHelper>();

        services.AddTransient<HmrcExecutionPolicy>();
        
        services.AddScoped<IPayeSchemesWithEnglishFractionService, PayeSchemesWithEnglishFractionService>();

        services.AddTransient<IPayeSchemesService, PayeSchemesService>();

        services.AddTransient<IAssociatedAccountsService, AssociatedAccountsService>();

        services.AddTransient<IStubAuthenticationService, StubAuthenticationService>();//TODO remove once gov login live

        return services;
    }
}
