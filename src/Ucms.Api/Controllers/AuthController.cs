namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions.Auth;
using Ucms.Application.Features.Auth.DTOs;
using Ucms.Application.Features.Auth.Queries;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities.Identity;
using Ucms.Domain.Enums;

/// <summary>
/// Autentifikatsiya va token boshqaruvi.
/// Аутентификация и управление токенами.
/// </summary>
[ApiController]
[Route("api/auth")]
[Tags("Auth")]
public class AuthController(
    UserManager<User>  userManager,
    ITokenService         tokenService,
    IUcmsDbContext         db) : ControllerBase
{
    /// <summary>
    /// Tizimga kirish (login).
    /// Вход в систему (login).
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var user = await userManager.FindByNameAsync(req.UserName)
                ?? await userManager.FindByEmailAsync(req.UserName);

        if (user is null || !await userManager.CheckPasswordAsync(user, req.Password))
            return Unauthorized(new { message = "Login yoki parol noto'g'ri. / Неверный логин или пароль." });

        if (await userManager.IsLockedOutAsync(user))
            return Unauthorized(new { message = "Hisob vaqtincha bloklangan. Keyinroq urinib ko'ring. / Аккаунт временно заблокирован. Попробуйте позже." });

        return Ok(await BuildAuthResponseAsync(user, ct));
    }

    /// <summary>
    /// Yangi foydalanuvchi ro'yxatdan o'tkazish.
    /// Регистрация нового пользователя.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        var user = new User
        {
            UserName       = req.UserName,
            Email          = req.Email,
            FullName       = req.FullName,
            OrganizationId = req.OrganizationId,
        };

        var result = await userManager.CreateAsync(user, req.Password);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return Ok(await BuildAuthResponseAsync(user, ct));
    }

    /// <summary>
    /// Access tokenni yangilash (refresh).
    /// Обновление access token (refresh).
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest req, CancellationToken ct)
    {
        var principal = tokenService.GetPrincipalFromExpiredToken(req.AccessToken);
        if (principal is null)
            return BadRequest(new { message = "Access token noto'g'ri. / Неверный access-токен." });

        var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out var uid))
            return BadRequest(new { message = "Token ichida foydalanuvchi topilmadi. / Пользователь в токене не найден." });

        var storedToken = await db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.UserId == uid && rt.Token == req.RefreshToken, ct);

        if (storedToken is null || !storedToken.IsActive)
            return Unauthorized(new { message = "Refresh token yaroqsiz yoki muddati o'tgan. / Refresh-токен недействителен или истёк." });

        var user = await userManager.FindByIdAsync(uid.ToString());
        if (user is null)
            return Unauthorized(new { message = "Foydalanuvchi topilmadi. / Пользователь не найден." });

        // Eski tokenni bekor qilish
        storedToken.IsRevoked = true;
        db.RefreshTokens.Update(storedToken);
        await db.SaveChangesAsync(ct);

        return Ok(await BuildAuthResponseAsync(user, ct));
    }

    /// <summary>
    /// Refresh tokenni bekor qilish (logout).
    /// Отзыв refresh token (выход из системы).
    /// </summary>
    [HttpPost("revoke")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Revoke(CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out var uid))
            return BadRequest(new { message = "Foydalanuvchi aniqlanmadi. / Пользователь не определён." });

        var tokens = await db.RefreshTokens
            .Where(rt => rt.UserId == uid && !rt.IsRevoked)
            .ToListAsync(ct);

        foreach (var t in tokens)
            t.IsRevoked = true;

        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// JWT access va refresh tokenlarni yaratib, AuthResponse qaytaradi.
    /// Генерирует JWT access и refresh токены, возвращает AuthResponse.
    /// </summary>
    private async Task<AuthResponse> BuildAuthResponseAsync(User user, CancellationToken ct)
    {
        var roles = await userManager.GetRolesAsync(user);

        // Tashkilot turini aniqlash — JWT ga org_type claim qo'shish uchun
        string? orgType = null;
        if (user.OrganizationId.HasValue)
        {
            var type = await db.Organizations
                .Where(o => o.Id == user.OrganizationId.Value)
                .Select(o => (OrganizationType?)o.Type)
                .FirstOrDefaultAsync(ct);

            orgType = type?.ToString(); // "Owner" yoki "Tenant"
        }

        var accessToken  = tokenService.GenerateAccessToken(user, roles, orgType);
        var refreshToken = tokenService.GenerateRefreshToken();

        var storedToken = new RefreshToken
        {
            UserId     = user.Id,
            Token      = refreshToken,
            ExpiresAt  = tokenService.GetRefreshTokenExpiry(),
            CreatedAt  = DateTimeOffset.UtcNow,
            DeviceInfo = HttpContext.Request.Headers.UserAgent.ToString(),
        };

        await db.RefreshTokens.AddAsync(storedToken, ct);
        await db.SaveChangesAsync(ct);

        return new AuthResponse(
            AccessToken:  accessToken,
            RefreshToken: refreshToken,
            ExpiresAt:    tokenService.GetAccessTokenExpiry(),
            UserId:       user.Id,
            UserName:     user.UserName!,
            FullName:     user.FullName,
            Roles:        [.. roles]
        );
    }
}
