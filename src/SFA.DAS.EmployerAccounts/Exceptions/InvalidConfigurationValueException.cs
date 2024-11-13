namespace SFA.DAS.EmployerAccounts.Exceptions;

[Serializable]
public class InvalidConfigurationValueException(string configurationItem) : Exception($"Configuration value for '{configurationItem}' is not valid.");