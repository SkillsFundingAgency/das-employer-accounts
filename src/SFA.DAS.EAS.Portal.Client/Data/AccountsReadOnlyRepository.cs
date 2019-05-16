using Microsoft.Azure.Documents;
using SFA.DAS.CosmosDb;
using SFA.DAS.EAS.Portal.Database;
using SFA.DAS.EAS.Portal.Database.Models;

namespace SFA.DAS.EAS.Portal.Client.Data
{
    public class AccountsReadOnlyRepository : ReadOnlyDocumentRepository<AccountDocument>, IAccountsReadOnlyRepository
    {
        public AccountsReadOnlyRepository(IDocumentClient documentClient)
            : base(documentClient, DocumentSettings.DatabaseName, DocumentSettings.AccountsCollectionName)
        {
        }
    }
}