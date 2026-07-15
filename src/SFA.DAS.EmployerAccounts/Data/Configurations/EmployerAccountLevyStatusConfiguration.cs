using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.EmployerAccounts.Models.LevyDormancy;

namespace SFA.DAS.EmployerAccounts.Data.Configurations;

public class EmployerAccountLevyStatusConfiguration : IEntityTypeConfiguration<EmployerAccountLevyStatus>
{
    public void Configure(EntityTypeBuilder<EmployerAccountLevyStatus> builder)
    {
        builder.ToTable("EmployerAccountLevyStatus");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.LastRefreshedAt).IsRequired();
        builder.HasIndex(e => e.AccountId).IsUnique();
    }
}
