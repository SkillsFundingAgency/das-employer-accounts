using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.EmployerAccounts.Models.LevyDormancy;

namespace SFA.DAS.EmployerAccounts.Data.Configurations;

public class LevyDormancyRequestConfiguration : IEntityTypeConfiguration<LevyDormancyRequest>
{
    public void Configure(EntityTypeBuilder<LevyDormancyRequest> builder)
    {
        builder.ToTable("LevyDormancyRequest");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(x => x.CreatedOn).IsRequired();
        builder.Property(x => x.UpdatedOn).IsRequired();

        builder.HasIndex(x => x.AccountId);
        builder.HasIndex(x => new { x.AccountId, x.Status });
    }
}
