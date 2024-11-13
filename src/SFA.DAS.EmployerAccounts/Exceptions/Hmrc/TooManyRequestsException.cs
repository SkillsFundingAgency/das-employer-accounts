namespace SFA.DAS.EmployerAccounts.Exceptions.Hmrc;

[Serializable]
public class TooManyRequestsException() : HttpException(429, "Rate limit has been reached");