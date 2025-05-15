using System.Collections.Generic;
using LegalEntity = SFA.DAS.EmployerAccounts.Api.Types.LegalEntity;

namespace SFA.DAS.EmployerAccounts.Api.Responses
{
    public record GetAllAccountLegalEntitiesResponse(PageInfo PageInfo, IEnumerable<LegalEntity> LegalEntities);
}