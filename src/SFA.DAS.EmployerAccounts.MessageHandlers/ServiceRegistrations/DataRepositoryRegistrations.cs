using SFA.DAS.EmployerAccounts.Data;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Interfaces;

namespace SFA.DAS.EmployerAccounts.MessageHandlers.ServiceRegistrations;

public static class DataRepositoryRegistrations
{
    public static IServiceCollection AddMessageHandlerDataRepositories(this IServiceCollection services)
    {
        services.AddScoped<IEmployerAccountRepository, EmployerAccountRepository>();
        services.AddScoped<IMembershipRepository, MembershipRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserAccountRepository, UserAccountRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();

        return services;
    }
}