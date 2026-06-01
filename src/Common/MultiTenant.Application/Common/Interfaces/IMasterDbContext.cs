using Microsoft.EntityFrameworkCore;
using MultiTenant.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MultiTenant.Application.Common.Interfaces;

public interface IMasterDbContext
{
    DbSet<Tenant> Tenants { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
