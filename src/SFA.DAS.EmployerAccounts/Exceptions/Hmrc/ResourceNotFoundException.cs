namespace SFA.DAS.EmployerAccounts.Exceptions.Hmrc;

[Serializable]
public class ResourceNotFoundException(string resourceUri) : HttpException(404, "Could not find requested resource - " + resourceUri);