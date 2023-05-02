﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Api.Common.AppStart;
using SFA.DAS.Api.Common.Configuration;
using SFA.DAS.Api.Common.Infrastructure;
using SFA.DAS.EmployerAccounts.Configuration;

namespace SFA.DAS.EmployerAccounts.Api.Authentication;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration config, bool isDevelopment = false)
    {
        if (isDevelopment)
        {
            services.AddAuthentication("BasicAuthentication")
                   .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
        }
        else
        {
            //var azureAdConfiguration = config
            //       .GetSection(ConfigurationKeys.AzureActiveDirectoryApiConfiguration)
            //       .Get<AzureActiveDirectoryConfiguration>();

            //var policies = new Dictionary<string, string> { { PolicyNames.Default, RoleNames.Default } };
            //services.AddAuthentication(azureAdConfiguration, policies);

            var azureAdConfiguration = config
                .GetSection(ConfigurationKeys.AzureActiveDirectoryApiConfiguration)
                .Get<AzureActiveDirectoryApiConfiguration>();
            
            services.AddAuthentication(auth =>
            {
                auth.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(auth =>
            {
                auth.Authority = $"https://login.microsoftonline.com/{azureAdConfiguration.Tenant}";
                auth.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidAudiences = azureAdConfiguration.Identifier.Split(",")
                };
            });
        }

        services.AddSingleton<IClaimsTransformation, AzureAdScopeClaimTransformation>();

        return services;
    }
}
