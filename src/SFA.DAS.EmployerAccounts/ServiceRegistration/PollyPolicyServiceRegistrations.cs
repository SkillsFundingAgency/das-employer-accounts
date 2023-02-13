﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using Polly.Timeout;
using SFA.DAS.EmployerAccounts.Configuration;

namespace SFA.DAS.EmployerAccounts.ServiceRegistration;

public static class PollyPolicyServiceRegistrations
{
    public static IServiceCollection AddPollyPolicy(this IServiceCollection services, EmployerAccountsConfiguration configuration)
    {
        services.AddTransient<IReadOnlyPolicyRegistry<string>>(provider =>
        {
            var logger = provider.GetService<ILogger>();
            var policyRegistry = new PolicyRegistry();
        
            var timeout = Policy
                .TimeoutAsync(TimeSpan.FromMilliseconds(configuration.DefaultServiceTimeoutMilliseconds), TimeoutStrategy.Pessimistic
                    , (pollyContext, timeSpan, task) =>
                    {
                        logger.LogWarning($"Error executing command for method {pollyContext.ExecutionKey} " +
                                          $"Reason: {task?.Exception?.Message}. " +
                                          $"Retrying in {timeSpan.Seconds} secs..."
                        );
                        return Task.CompletedTask;
                    }
                );

            policyRegistry.Add(EmployerAccounts.Constants.DefaultServiceTimeout, timeout);

            return policyRegistry;
        });

        return services;
    }

    private static PolicyRegistry GetPolicyRegistry(IServiceProvider context, EmployerAccountsConfiguration config)
    {
        var logger = context.GetService<ILogger>();
        var policyRegistry = new PolicyRegistry();
        
        var timeout = Policy
            .TimeoutAsync(TimeSpan.FromMilliseconds(config.DefaultServiceTimeoutMilliseconds), TimeoutStrategy.Pessimistic
                , (pollyContext, timeSpan, task) =>
                {
                    logger.LogWarning($"Error executing command for method {pollyContext.ExecutionKey} " +
                                $"Reason: {task?.Exception?.Message}. " +
                                $"Retrying in {timeSpan.Seconds} secs..."
                    );
                    return Task.CompletedTask;
                }
            );

        policyRegistry.Add(EmployerAccounts.Constants.DefaultServiceTimeout, timeout);

        return policyRegistry;
    }
}