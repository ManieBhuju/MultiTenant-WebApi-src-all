using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiTenant.Domain.Entities;


namespace MultiTenant.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(t => t.EmailAddress)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.TenantId)
            .IsRequired()
            .HasMaxLength(4);

        builder.Property(t => t.DbConnStr)
            .IsRequired();

        builder.HasIndex(t => t.TenantId)
            .IsUnique();
    }
}
