using MediatR;
using MultiTenant.Application.Common.Interfaces;
using MultiTenant.Application.Common.Models;
using MultiTenant.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MultiTenant.Application.Features.Tenants.Commands.DeleteTenant;

public record DeleteTenantCommand(string Id) : IRequest<ServiceResult<bool>>;

public class DeleteTenantCommandHandler : IRequestHandler<DeleteTenantCommand, ServiceResult<bool>>
{
    private readonly IMasterDbContext _db;
    private readonly ITenantService _tenantService;

    public DeleteTenantCommandHandler(IMasterDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<ServiceResult<bool>> Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (tenant == null)
            return ServiceResult.Failed<bool>(ServiceError.NotFound);
        await _db.SaveChangesAsync(cancellationToken);
        // Delegate the Delete to TenantService
        //var result = await _tenantService.DeleteTenantAsync(tenant.Id, cancellationToken);
        //if (!result.Succeeded)
        //    return ServiceResult.Failed<bool>(result.Errors.ToArray());

        return ServiceResult.Success(true);
    }
}
