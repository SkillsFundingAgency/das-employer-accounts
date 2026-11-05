using Microsoft.Extensions.Configuration;
using SFA.DAS.EmployerAccounts.Configuration;

namespace SFA.DAS.EmployerAccounts.Services;

public class Feature(IConfiguration configuration) : IFeature
{
    private readonly Features _features = configuration.Get<Features>() ?? new Features();

    public bool IsFeatureEnabled(string featureName)
    {
        return featureName switch
        {
            FeatureNames.LevyProjectionTransparency => _features.LevyProjectionTransparency,
            // add other features here
            _ => false
        };
    }
}