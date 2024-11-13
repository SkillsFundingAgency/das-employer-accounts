namespace SFA.DAS.EmployerAccounts.Exceptions;

[Serializable]
public class InvalidOrganisationTypeConversionException(string message) : Exception(message);