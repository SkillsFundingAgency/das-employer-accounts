namespace SFA.DAS.EmployerAccounts.Audit;

[Serializable]
public class InvalidContextException(string message) : Exception(message);
