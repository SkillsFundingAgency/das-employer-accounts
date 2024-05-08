using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.EmployerAccounts.Models.PAYE;

namespace SFA.DAS.EmployerAccounts.Data.Configurations;

public class PayeConfiguration: IEntityTypeConfiguration<Paye>
{
    public void Configure(EntityTypeBuilder<Paye> builder)
    {
        builder.ToTable("Paye");
        builder.Ignore(a => a.AccountId);
        
        // Primary Key
        builder.HasKey(e => e.EmpRef);
        
        // Column mappings
        builder.Property(e => e.EmpRef).HasColumnName("Ref").HasMaxLength(16).IsRequired();
        builder.Property(e => e.AccessToken).HasColumnName("AccessToken").HasMaxLength(50);
        builder.Property(e => e.RefreshToken).HasColumnName("RefreshToken").HasMaxLength(50);
        builder.Property(e => e.RefName).HasColumnName("Name").HasMaxLength(500);
        builder.Property(e => e.Aorn).HasColumnName("Aorn").HasMaxLength(25);
    }
}