using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiTenant.Domain.Entities;

namespace MultiTenant.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        
        builder.Property(u => u.TenantId)
               .HasMaxLength(36)
               .IsRequired(false);

        builder.HasOne(x => x.Tenant)
               .WithMany()
               .HasForeignKey(x => x.TenantId)
               .HasPrincipalKey(t => t.Id)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
