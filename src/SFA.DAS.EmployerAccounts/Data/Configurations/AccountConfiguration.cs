﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.EmployerAccounts.Models.Account;

namespace SFA.DAS.EmployerAccounts.Data.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.Ignore(a => a.role).Ignore(a => a.RoleName);

        builder.HasMany(a => a.AccountLegalEntities);
        builder.HasMany(a => a.Memberships);
        builder.HasMany(a => a.AccountHistory);
    }
}