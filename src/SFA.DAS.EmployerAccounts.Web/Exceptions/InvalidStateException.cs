namespace SFA.DAS.EmployerAccounts.Web.Exceptions;

[Serializable]
public class InvalidStateException(string message) : Exception(message);