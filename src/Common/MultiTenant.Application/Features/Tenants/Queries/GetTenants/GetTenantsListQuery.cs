using MediatR;
using MultiTenant.Application.Common.Models;
using MultiTenant.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using MultiTenant.Application.Common.Interfaces;

namespace MultiTenant.Application.Features.Tenants.Queries.GetTenants;

public record GetTenantsListQuery() : IRequest<ServiceResult<IEnumerable<TenantDto>>>;

public class GetTenantsQueryHandler : IRequestHandler<GetTenantsListQuery, ServiceResult<IEnumerable<TenantDto>>>
{
    private readonly IMasterDbContext _db;

    public GetTenantsQueryHandler(IMasterDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceResult<IEnumerable<TenantDto>>> Handle(GetTenantsListQuery request, CancellationToken cancellationToken)
    {
        var tenants = await _db.Tenants.AsNoTracking()
            .Select(t => new TenantDto { Id = t.Id, Name = t.Name, EmailAddress = t.EmailAddress, TenantId = t.TenantId, DbConnStr = t.DbConnStr })
            .ToListAsync(cancellationToken);

        return ServiceResult.Success<IEnumerable<TenantDto>>(tenants);
    }
}
