using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerAccounts.Models.Account;

namespace SFA.DAS.EmployerAccounts.Api.Requests
{
    public record GetAllLegalEntitiesRequest
    {
        [Required]
        public List<long> AccountIds { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortColumn { get; set; } = nameof(AccountLegalEntity.Name);
        public bool IsAscending { get; set; } = false;
    }
}