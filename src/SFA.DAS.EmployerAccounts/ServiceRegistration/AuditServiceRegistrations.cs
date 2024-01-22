using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerAccounts.Audit;
using SFA.DAS.EmployerAccounts.Audit.MessageBuilders;
using SFA.DAS.EmployerAccounts.Configuration;

namespace SFA.DAS.EmployerAccounts.ServiceRegistration;

public static class AuditServiceRegistrations
{
    public static IServiceCollection AddAuditServices(this IServiceCollection services)
    {
        services.AddTransient<IAuditMessageBuilder, BaseAuditMessageBuilder>();
        services.AddTransient<IAuditMessageBuilder, ChangedByMessageBuilder>();

        services.AddTransient<IAuditClient, AuditClient>();
        services.AddTransient<IAuditService, AuditService>();

        return services;
    }
}