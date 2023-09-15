﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.ReferenceData.Api.Client;

namespace SFA.DAS.EmployerAccounts.ServiceRegistration;

public static class ReferenceDataServiceRegistrations
{
    public static IServiceCollection AddReferenceDataApi(this IServiceCollection services)
    {
        services.AddScoped<IReferenceDataService, ReferenceDataService>();
        services.AddScoped<IReferenceDataApiClient>(sp => new ReferenceDataApiClient(sp.GetService<IReferenceDataApiConfiguration>()));

        return services;
    }
}