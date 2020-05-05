﻿using System.Threading.Tasks;

namespace SFA.DAS.EmployerAccounts.Interfaces
{
    public interface IClientContentService
    {
        Task<string> GetContent(string type, string clientId);
    }
}
