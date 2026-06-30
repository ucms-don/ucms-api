namespace Ucms.Application.Abstractions.Authorization;

public interface IPermissionProvider
{
    Task<bool> HasPermissionAsync(string permission, CancellationToken cancellationToken = default);
}
