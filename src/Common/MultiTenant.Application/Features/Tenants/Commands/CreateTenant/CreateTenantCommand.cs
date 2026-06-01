using MediatR;
using MultiTenant.Application.Common.Interfaces;
using MultiTenant.Application.Common.Models;
using MultiTenant.Application.DTOs;
using MultiTenant.Domain.Entities;

namespace MultiTenant.Application.Features.Tenants.Commands.CreateTenant;

public record CreateTenantCommand(string Name, string EmailAddress, string TenantId) : IRequest<ServiceResult<TenantDto>>;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, ServiceResult<TenantDto>>
{
    private readonly IMasterDbContext _db;
    private readonly ITenantService _tenantService;

    public CreateTenantCommandHandler(IMasterDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<ServiceResult<TenantDto>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            EmailAddress = request.EmailAddress,
            TenantId = request.TenantId,
            DbConnStr = string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync(cancellationToken);

        // Create new tenant database, migrations and default admin user via infrastructure service
        var tenantResult = await _tenantService.CreateTenantAsync(tenant, cancellationToken);
        if (!tenantResult.Succeeded)
        {
            // rollback tenant record
            _db.Tenants.Remove(tenant);
            await _db.SaveChangesAsync(cancellationToken);
            return ServiceResult.Failed<TenantDto>(tenantResult.Errors.ToArray());
        }

        tenant.DbConnStr = tenantResult.Data;
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            EmailAddress = tenant.EmailAddress,
            TenantId = tenant.TenantId,
            DbConnStr = tenant.DbConnStr
        };

        return ServiceResult.Success(dto);
    }
}
