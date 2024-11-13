namespace SFA.DAS.EmployerAccounts.Exceptions.Hmrc;

[Serializable]
public class RequestTimeOutException() : HttpException(408, "Request has time out");