using Newtonsoft.Json;

namespace SFA.DAS.EmployerAccounts.Exceptions;

[Serializable]
public class InvalidRequestException(Dictionary<string, string> errorMessages) : Exception(BuildErrorMessage(errorMessages))
{
    public Dictionary<string, string> ErrorMessages { get; private set; } = errorMessages;

    private static string BuildErrorMessage(Dictionary<string, string> errorMessages)
    {
        if (errorMessages.Count == 0)
        {
            errorMessages.Add("Request", "Request is invalid");
        }
        return JsonConvert.SerializeObject(errorMessages, Formatting.Indented);
    }
}