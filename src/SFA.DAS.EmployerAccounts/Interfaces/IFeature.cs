namespace SFA.DAS.EmployerAccounts.Interfaces;

public interface IFeature
{
    bool IsFeatureEnabled(string featureName);
}