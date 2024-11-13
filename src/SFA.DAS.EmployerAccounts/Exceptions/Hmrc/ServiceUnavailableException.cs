namespace SFA.DAS.EmployerAccounts.Exceptions.Hmrc;

[Serializable]
public class ServiceUnavailableException() : HttpException(503, "Service is unavailable");