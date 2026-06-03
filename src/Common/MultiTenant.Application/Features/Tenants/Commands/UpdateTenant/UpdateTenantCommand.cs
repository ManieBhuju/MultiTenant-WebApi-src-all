using MediatR;
using MultiTenant.Application.Common.Interfaces;
using MultiTenant.Application.Common.Models;
using MultiTenant.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace MultiTenant.Application.Features.Tenants.Commands.UpdateTenant;

public record UpdateTenantCommand(string Id, string? Name, string? EmailAddress, string? TenantId, string? DbConnStr) : IRequest<ServiceResult<TenantDto>>;

public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, ServiceResult<TenantDto>>
{
    private readonly IMasterDbContext _db;
    private readonly ITenantService _tenantService;

    public UpdateTenantCommandHandler(IMasterDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<ServiceResult<TenantDto>> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (tenant == null)
            return ServiceResult.Failed<TenantDto>(ServiceError.NotFound);

        var oldTenantId = tenant.TenantId;

        // apply updates only for provided values
        if (!string.IsNullOrWhiteSpace(request.Name))
            tenant.Name = request.Name!;
        if (!string.IsNullOrWhiteSpace(request.EmailAddress))
            tenant.EmailAddress = request.EmailAddress!;
        if (!string.IsNullOrWhiteSpace(request.TenantId))
            tenant.TenantId = request.TenantId!;
        if (!string.IsNullOrWhiteSpace(request.DbConnStr))
            tenant.DbConnStr = request.DbConnStr!;
        await _db.SaveChangesAsync(cancellationToken);
        // Delegate the update to TenantService
        //var updateResult = await _tenantService.UpdateTenantAsync(tenant, oldTenantId, cancellationToken);
        //if (!updateResult.Succeeded)
        //    return ServiceResult.Failed<TenantDto>(updateResult.Errors.ToArray());

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
