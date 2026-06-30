namespace Ucms.Domain.Entities.Identity;

using Microsoft.AspNetCore.Identity;

public class UserRole : IdentityUserRole<Guid>
{
    public virtual User User { get; set; } = default!;
    public virtual Role Role { get; set; } = default!;
}
