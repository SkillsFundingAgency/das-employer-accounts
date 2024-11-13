namespace SFA.DAS.EmployerAccounts.Exceptions.Hmrc;

[Serializable]
public class InternalServerErrorException() : HttpException(500, "Internal server error");