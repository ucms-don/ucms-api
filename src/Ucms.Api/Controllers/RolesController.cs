namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;
using Ucms.Domain.Constants;
using Ucms.Domain.Entities.Identity;

/// <summary>
/// Rollarni va ularga biriktirilgan ruxsatlarni (permissions) boshqarish.
/// Управление ролями и привязанными к ним разрешениями.
/// </summary>
[ApiController]
[Route("api/roles")]
[Tags("Roles")]
[Authorize(Roles = "Admin")]
public class RolesController(
    RoleManager<Role> roleManager,
    IUcmsDbContext     db) : ControllerBase
{
    // ── DTOs ─────────────────────────────────────────────────────────────────

    public record RoleItem(
        Guid   Id,
        string Name,
        string? Description,
        int    UsersCount,
        IReadOnlyList<string> Permissions);

    public record CreateRoleRequest(string Name, string? Description);

    public record UpdateRoleRequest(string Name, string? Description);

    public record SetPermissionsRequest(IReadOnlyList<string> Permissions);

    public record PermissionGroupDto(string Key, string Label, IReadOnlyList<string> Permissions);

    // ── Endpoints ─────────────────────────────────────────────────────────────

    /// <summary>Barcha rollar ro'yxati (foydalanuvchilar soni va permissionslar bilan).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RoleItem>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var roles = await roleManager.Roles
            .AsNoTracking()
            .ToListAsync(ct);

        var result = new List<RoleItem>();
        foreach (var role in roles)
        {
            var usersCount = await db.UserRoles.CountAsync(ur => ur.RoleId == role.Id, ct);
            var perms = await db.RoleClaims
                .Where(rc => rc.RoleId == role.Id && rc.ClaimType == Permissions.ClaimType)
                .Select(rc => rc.ClaimValue!)
                .ToListAsync(ct);

            result.Add(new RoleItem(role.Id, role.Name!, role.Description, usersCount, perms));
        }

        return Ok(result);
    }

    /// <summary>Yangi rol yaratish.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(RoleItem), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest req)
    {
        var role = new Role(req.Name) { Description = req.Description };
        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return CreatedAtAction(nameof(GetAll), null,
            new RoleItem(role.Id, role.Name!, role.Description, 0, []));
    }

    /// <summary>Rolni yangilash.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleRequest req)
    {
        var role = await roleManager.FindByIdAsync(id.ToString());
        if (role is null) return NotFound();

        role.Name        = req.Name;
        role.Description = req.Description;
        role.NormalizedName = req.Name.ToUpperInvariant();

        var result = await roleManager.UpdateAsync(role);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return NoContent();
    }

    /// <summary>Rolni o'chirish (foydalanuvchilar bog'lanmagan bo'lsa).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var role = await roleManager.FindByIdAsync(id.ToString());
        if (role is null) return NotFound();

        var hasUsers = await db.UserRoles.AnyAsync(ur => ur.RoleId == id, ct);
        if (hasUsers)
            return BadRequest(new { message = "Bu rolda foydalanuvchilar bor, o'chirib bo'lmaydi. / Роль используется пользователями." });

        await roleManager.DeleteAsync(role);
        return NoContent();
    }

    /// <summary>Roldagi permissionslarni o'rnatish (bulk replace).</summary>
    [HttpPut("{id:guid}/permissions")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SetPermissions(Guid id, [FromBody] SetPermissionsRequest req, CancellationToken ct)
    {
        var role = await roleManager.FindByIdAsync(id.ToString());
        if (role is null) return NotFound();

        // Faqat ma'lum permissionslarni qabul qilamiz
        var valid = req.Permissions
            .Where(p => Permissions.All.Contains(p))
            .Distinct()
            .ToList();

        // Mavjud permission claimlarini o'chiramiz
        var existing = await db.RoleClaims
            .Where(rc => rc.RoleId == id && rc.ClaimType == Permissions.ClaimType)
            .ToListAsync(ct);

        db.RoleClaims.RemoveRange(existing);

        // Yangilarini qo'shamiz
        var newClaims = valid.Select(p => new RoleClaim
        {
            RoleId     = id,
            ClaimType  = Permissions.ClaimType,
            ClaimValue = p,
        });

        await db.RoleClaims.AddRangeAsync(newClaims, ct);
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    /// <summary>Barcha mavjud statik permissionslar ro'yxati (UI checkbox uchun).</summary>
    [HttpGet("/api/permissions")]
    [ProducesResponseType(typeof(IReadOnlyList<PermissionGroupDto>), 200)]
    public IActionResult GetAllPermissions()
    {
        var groups = Permissions.Groups
            .Select(g => new PermissionGroupDto(g.Key, g.Label, g.Permissions))
            .ToList();

        return Ok(groups);
    }
}
