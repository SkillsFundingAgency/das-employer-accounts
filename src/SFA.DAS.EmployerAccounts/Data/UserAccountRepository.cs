﻿using System.Data;
using System.Data.Entity;
using Dapper;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Sql.Client;

namespace SFA.DAS.EmployerAccounts.Data;

public class UserAccountRepository : BaseRepository, IUserAccountRepository
{
    private readonly Lazy<EmployerAccountsDbContext> _db;

    public UserAccountRepository(EmployerAccountsConfiguration configuration, ILog logger, Lazy<EmployerAccountsDbContext> db)
        : base(configuration.DatabaseConnectionString, logger)
    {
        _db = db;
    }

    public async Task<Accounts<Account>> GetAccountsByUserRef(string userRef)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@userRef", Guid.Parse(userRef), DbType.Guid);

        var result = await _db.Value.Database.GetDbConnection().QueryAsync<Account>(
            sql: @"[employer_account].[GetAccounts_ByUserRef]",
            param: parameters,
            commandType: CommandType.StoredProcedure);

        return new Accounts<Account>
        {
            AccountList = (List<Account>) result
        };
    }

    public async Task<User> Get(string email)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@email", email, DbType.String);

        var result = await _db.Value.Database.GetDbConnection().QueryAsync<User>(
            sql: "SELECT Id, CONVERT(NVARCHAR(50), UserRef) AS UserRef, Email, FirstName, LastName, CorrelationId FROM [employer_account].[User] WHERE Email = @email;",
            param: parameters,
            commandType: CommandType.Text);

        return result.SingleOrDefault();
    }

    public async Task<User> Get(long id)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@id", id, DbType.Int64);

        var result = await _db.Value.Database.GetDbConnection().QueryAsync<User>(
            sql: "SELECT Id, CONVERT(NVARCHAR(50), UserRef) AS UserRef, Email, FirstName, LastName, CorrelationId FROM [employer_account].[User] WHERE Id = @id;",
            param: parameters,
            commandType: CommandType.Text);

        return result.SingleOrDefault();
    }


    public Task<User> GetUserByRef(Guid @ref)
    {
        return _db.Value.Users.SingleOrDefaultAsync(u => u.Ref == @ref);
    }

    public async Task<Accounts<Account>> GetAccounts()
    {
        var accountList = await _db.Value.Accounts.ToListAsync();
            
        return new Accounts<Account>
        {
            AccountList = accountList,                
            AccountsCount = accountList.Count()
        };
    }

    public async Task<User> GetByEmailAddress(string emailAddress)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@email", emailAddress, DbType.String);

        var result = await _db.Value.Database.GetDbConnection().QueryAsync<User>(
            sql: "SELECT Id, CONVERT(varchar(64), UserRef) as UserRef, Email, FirstName, LastName, CorrelationId FROM [employer_account].[User] WHERE Email = @email",
            param: parameters,
            commandType: CommandType.Text);

        return result.SingleOrDefault();
    }

    public Task Upsert(User user)
    {

        var parameters = new DynamicParameters();

        parameters.Add("@email", user.Email, DbType.String);
        parameters.Add("@userRef", new Guid(user.UserRef), DbType.Guid);
        parameters.Add("@firstName", user.FirstName, DbType.String);
        parameters.Add("@lastName", user.LastName, DbType.String);
        parameters.Add("@correlationId", user.CorrelationId, DbType.String);

        return _db.Value.Database.GetDbConnection().ExecuteAsync(
            sql: "[employer_account].[UpsertUser] @userRef, @email, @firstName, @lastName, @correlationId",
            param: parameters,
            commandType: CommandType.Text);
    }
}