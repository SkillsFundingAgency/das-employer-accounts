using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.ApplicationInsights;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.AutoConfiguration.DependencyResolution;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.EmployerAccounts.AppStart;
using SFA.DAS.EmployerAccounts.Data;
using SFA.DAS.EmployerAccounts.Mappings;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAccount;
using SFA.DAS.EmployerAccounts.ServiceRegistration;
using SFA.DAS.EmployerAccounts.Services;
using SFA.DAS.EmployerAccounts.Startup;
using SFA.DAS.EmployerAccounts.Web.Extensions;
using SFA.DAS.EmployerAccounts.Web.Filters;
using SFA.DAS.EmployerAccounts.Web.RouteValues;
using SFA.DAS.EmployerAccounts.Web.StartupExtensions;
using SFA.DAS.GovUK.Auth.AppStart;
using SFA.DAS.GovUK.Auth.Extensions;
using SFA.DAS.GovUK.Auth.Models;
using SFA.DAS.NServiceBus.Features.ClientOutbox.Data;
using SFA.DAS.UnitOfWork.DependencyResolution.Microsoft;
using SFA.DAS.UnitOfWork.EntityFrameworkCore.DependencyResolution.Microsoft;
using SFA.DAS.UnitOfWork.Mvc.Extensions;
using SFA.DAS.UnitOfWork.NServiceBus.Features.ClientOutbox.DependencyResolution.Microsoft;

namespace SFA.DAS.EmployerAccounts.Web;

public class Startup
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public Startup(IConfiguration configuration, IWebHostEnvironment environment, bool buildConfig = true)
    {
        _environment = environment;
        _configuration = buildConfig ? configuration.BuildDasConfiguration() : configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(_configuration);

        services.AddOptions();

        services.AddLogging(builder =>
        {
            builder.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Information);
            builder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Information);
        });

        services.AddHttpContextAccessor();

        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        services.AddConfigurationOptions(_configuration);

        var employerAccountsConfiguration = _configuration.GetSection(ConfigurationKeys.EmployerAccounts).Get<EmployerAccountsConfiguration>();

        services.AddOrchestrators();
        services.AddAutoMapper(typeof(Startup).Assembly, typeof(AccountMappings).Assembly);
        services.AddAutoConfiguration();
        services.AddDatabaseRegistration();
        services.AddDataRepositories();
        services.AddApplicationServices();
        services.AddHmrcServices();

        services.AddMaMenuConfiguration(RouteNames.SignOut, _configuration["ResourceEnvironmentName"]);

        services.AddAuditServices();
        services.AddCachesRegistrations();
        services.AddDateTimeServices(_configuration);

        services
            .AddUnitOfWork()
            .AddEntityFramework(employerAccountsConfiguration)
            .AddEntityFrameworkUnitOfWork<EmployerAccountsDbContext>();

        services.AddNServiceBusClientUnitOfWork();
        services.AddEmployerAccountsApi();
        services.AddExecutionPolicies();
        services.AddEmployerAccountsOuterApi(employerAccountsConfiguration.EmployerAccountsOuterApiConfiguration);
        services.AddCommittmentsV2Client(employerAccountsConfiguration.CommitmentsApi);
        services.AddPollyPolicy(employerAccountsConfiguration);
        services.AddContentApiClient(employerAccountsConfiguration);
        services.AddApprenticeshipLevyApi(employerAccountsConfiguration);

        services.AddAuthenticationServices();

        services.AddMediatorValidators();
        services.AddMediatR(serviceConfiguration => serviceConfiguration.RegisterServicesFromAssembly(typeof(GetEmployerAccountByIdQuery).Assembly));

        var govConfig = _configuration.GetSection("SFA.DAS.Employer.GovSignIn");
        govConfig["ResourceEnvironmentName"] = _configuration["ResourceEnvironmentName"];
        govConfig["StubAuth"] = _configuration["StubAuth"];

        services.AddAndConfigureGovUkAuthentication(govConfig, new AuthRedirects
        {
            SignedOutRedirectUrl = "",
            LocalStubLoginPath = "/service/SignIn-Stub"
        }, null, typeof(UserAccountService));

        services.Configure<IISServerOptions>(options => { options.AutomaticAuthentication = false; });

        services.Configure<RouteOptions>(options => { }).AddMvc(options =>
        {
            options.Filters.Add(new AnalyticsFilterAttribute());
            if (!_configuration.IsDev())
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            }
        });

        services.AddApplicationInsightsTelemetry();

        services.AddDataProtection(_configuration, _environment.IsDevelopment());

        if (!_environment.IsDevelopment())
        {
            services.AddHealthChecks();
        }

#if DEBUG
        services.AddControllersWithViews(o => { }).AddRazorRuntimeCompilation();
#endif

        services.AddValidatorsFromAssembly(typeof(Startup).Assembly);
    }

    public void ConfigureContainer(UpdateableServiceProvider serviceProvider)
    {
        serviceProvider.StartNServiceBus(_configuration.IsDevOrLocal(), ServiceBusEndpointType.Web);

        // Replacing ClientOutboxPersisterV2 with a local version to fix unit of work issue due to propogating Task up the chain rather than awaiting on DB Command.
        // not clear why this fixes the issue. Attempted to make the change in SFA.DAS.Nservicebus.SqlServer however it conflicts when upgraded with SFA.DAS.UnitOfWork.Nservicebus
        // which would require upgrading to NET6 to resolve.
        var serviceDescriptor = serviceProvider.FirstOrDefault(serv => serv.ServiceType == typeof(IClientOutboxStorageV2));
        serviceProvider.Remove(serviceDescriptor);
        serviceProvider.AddScoped<IClientOutboxStorageV2, ClientOutboxPersisterV2>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/error");
        }

        app.UseUnitOfWork();

        app.UseStaticFiles();

        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseMiddleware<RobotsTextMiddleware>();

        app.UseAuthentication();
        app.UseCookiePolicy(new CookiePolicyOptions
        {
            Secure = CookieSecurePolicy.Always,
            MinimumSameSitePolicy = SameSiteMode.None,
            HttpOnly = HttpOnlyPolicy.Always
        });
        
        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapSessionKeepAliveEndpoint();
            endpoints.MapDefaultControllerRoute();
        });
    }
}