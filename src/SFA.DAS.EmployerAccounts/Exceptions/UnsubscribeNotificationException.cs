namespace SFA.DAS.EmployerAccounts.Exceptions;

[Serializable]
public class UnsubscribeNotificationException(string message) : Exception(message);