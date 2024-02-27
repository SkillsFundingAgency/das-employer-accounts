using System.Text.Json.Serialization;

namespace SFA.DAS.EmployerAccounts.Interfaces.OuterApi
{
    public interface IPostApiRequest : IPostApiRequest<object>
    {
    }

    public interface IPostApiRequest<TData>
    {
        [JsonIgnore]
        string PostUrl { get; }
        TData Data { get; set; }
    }
}
