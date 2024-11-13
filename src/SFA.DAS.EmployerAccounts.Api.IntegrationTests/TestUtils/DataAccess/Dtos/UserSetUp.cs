﻿namespace SFA.DAS.EmployerAccounts.Api.IntegrationTests.TestUtils.DataAccess.Dtos;

public class UserSetup
{
    public UserInput UserInput { get; set; }
    public UserOutput UserOutput { get; set; }
    public List<EmployerAccountSetup> Accounts { get; } = [];
}