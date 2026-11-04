using Microsoft.Extensions.Configuration;

namespace SFA.DAS.EmployerAccounts.Services;

public class Feature(IConfiguration configuration) : IFeature
{
    public bool IsFeatureEnabled(string feature)
    {
        string featureValue = configuration[$"Features:{feature}"];
        return !string.IsNullOrWhiteSpace(featureValue) && bool.Parse(featureValue);
    }
}