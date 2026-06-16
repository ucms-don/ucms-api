namespace Ucms.Infrastructure.Persistence;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;
using Polly.Retry;
using Ucms.Domain.Entities;
using Ucms.Domain.Entities.Identity;
using Ucms.Domain.Enums;

/// <summary>
/// Tizimni birinchi ishga tushirishda zaruriy ma'lumotlarni yaratadi.
/// Idempotent: mavjud bo'lsa qayta yaratmaydi.
/// </summary>
public class UcmsDbContextSeed
{
    // ══════════════════════════════════════════════════════════════════════════
    // Fixed GUIDs (idempotent seeding uchun)
    // ══════════════════════════════════════════════════════════════════════════

    // ── Rollar ────────────────────────────────────────────────────────────────
    private static readonly Guid AdminRoleId      = new("00000000-0000-0000-0000-000000000010");
    private static readonly Guid ManagerRoleId    = new("00000000-0000-0000-0000-000000000011");
    private static readonly Guid BrigadirRoleId   = new("00000000-0000-0000-0000-000000000012");
    private static readonly Guid AccountantRoleId = new("00000000-0000-0000-0000-000000000013");

    // ── OWNER tashkilot (tizim egasi — UCMS) ─────────────────────────────────
    private static readonly Guid OwnerOrgId      = new("00000000-0000-0000-0001-000000000000");
    private static readonly Guid SysAdminId      = new("00000000-0000-0000-0000-000000000001");
    private static readonly Guid OwnerBrigadeId  = new("00000000-0000-0000-0003-000000000001");
    private static readonly Guid OwnerEmployeeId = new("00000000-0000-0000-0010-000000000001");

    // ── TENANT 1 — "Ihtiyor Qurilish Kompaniyasi" ────────────────────────────────
    private static readonly Guid T1OrgId        = new("00000000-0000-0000-0001-000000000001");
    private static readonly Guid T1AdminId      = new("00000000-0000-0000-0000-000000000101");
    private static readonly Guid T1ManagerId    = new("00000000-0000-0000-0000-000000000102");
    private static readonly Guid T1BrigadirId   = new("00000000-0000-0000-0000-000000000103");
    private static readonly Guid T1AccountantId = new("00000000-0000-0000-0000-000000000104");

    private static readonly Guid T1Project1Id   = new("00000000-0000-0000-0002-000000000101");
    private static readonly Guid T1Project2Id   = new("00000000-0000-0000-0002-000000000102");

    private static readonly Guid T1Brigade1Id   = new("00000000-0000-0000-0003-000000000101");
    private static readonly Guid T1Brigade2Id   = new("00000000-0000-0000-0003-000000000102");

    // Estimate documents
    private static readonly Guid T1P1Est1Id     = new("00000000-0000-0000-000B-000000000101");
    private static readonly Guid T1P2Est1Id     = new("00000000-0000-0000-000B-000000000201");

    // Project 1 estimate
    private static readonly Guid T1P1Sec1Id     = new("00000000-0000-0000-0004-000000000101");
    private static readonly Guid T1P1Sec2Id     = new("00000000-0000-0000-0004-000000000102");
    private static readonly Guid T1P1Item1Id    = new("00000000-0000-0000-0005-000000000101");
    private static readonly Guid T1P1Item2Id    = new("00000000-0000-0000-0005-000000000102");
    private static readonly Guid T1P1Item3Id    = new("00000000-0000-0000-0005-000000000103");
    private static readonly Guid T1P1Item4Id    = new("00000000-0000-0000-0005-000000000104");
    private static readonly Guid T1P1Item5Id    = new("00000000-0000-0000-0005-000000000105");

    // Project 2 estimate
    private static readonly Guid T1P2Sec1Id     = new("00000000-0000-0000-0004-000000000201");
    private static readonly Guid T1P2Item1Id    = new("00000000-0000-0000-0005-000000000201");
    private static readonly Guid T1P2Item2Id    = new("00000000-0000-0000-0005-000000000202");

    // WorkLogs
    private static readonly Guid T1WL1Id        = new("00000000-0000-0000-0006-000000000101");
    private static readonly Guid T1WL2Id        = new("00000000-0000-0000-0006-000000000102");
    private static readonly Guid T1WL3Id        = new("00000000-0000-0000-0006-000000000103");
    private static readonly Guid T1WL4Id        = new("00000000-0000-0000-0006-000000000104");
    private static readonly Guid T1WL5Id        = new("00000000-0000-0000-0006-000000000105");

    // BrigadePayment
    private static readonly Guid T1BP1Id        = new("00000000-0000-0000-0007-000000000101");

    // ClientAct
    private static readonly Guid T1Act1Id       = new("00000000-0000-0000-0008-000000000101");

    // ClientPayment
    private static readonly Guid T1CP1Id        = new("00000000-0000-0000-0009-000000000101");

    // ── IXTIYOR — alohida qurilish pudratchisi tashkiloti ───────────────────────
    private static readonly Guid IhtiyorOrgId          = new("00000000-0000-0000-0001-000000000002");
    private static readonly Guid IhtiyorDirectorUserId  = new("00000000-0000-0000-0000-000000000201");
    private static readonly Guid IhtiyorDirectorEmpId   = new("00000000-0000-0000-0010-000000000002");
    private static readonly Guid IhtiyorWorkerEmpId     = new("00000000-0000-0000-0010-000000000003");
    private static readonly Guid IhtiyorBrigadeId       = new("00000000-0000-0000-0003-000000000201");

    private static readonly Guid IhtiyorProjectId   = new("00000000-0000-0000-0002-000000000201");
    private static readonly Guid IhtiyorEstId       = new("00000000-0000-0000-000B-000000000301");
    private static readonly Guid IhtiyorSec1Id      = new("00000000-0000-0000-0004-000000000301");
    private static readonly Guid IhtiyorSec2Id      = new("00000000-0000-0000-0004-000000000302");
    private static readonly Guid IhtiyorItem1Id     = new("00000000-0000-0000-0005-000000000301");
    private static readonly Guid IhtiyorItem2Id     = new("00000000-0000-0000-0005-000000000302");
    private static readonly Guid IhtiyorItem3Id     = new("00000000-0000-0000-0005-000000000303");
    private static readonly Guid IhtiyorItem4Id     = new("00000000-0000-0000-0005-000000000304");
    private static readonly Guid IhtiyorItem5Id     = new("00000000-0000-0000-0005-000000000305");

    // ── O'lchov birliklari (fixed — idempotent seeding uchun) ─────────────────
    private static readonly Guid UnitM2Id   = new("00000000-0000-0000-000A-000000000001");
    private static readonly Guid UnitM3Id   = new("00000000-0000-0000-000A-000000000002");
    private static readonly Guid UnitMId    = new("00000000-0000-0000-000A-000000000003");
    private static readonly Guid UnitMpId   = new("00000000-0000-0000-000A-000000000004");
    private static readonly Guid UnitDonaId = new("00000000-0000-0000-000A-000000000005");
    private static readonly Guid UnitKgId   = new("00000000-0000-0000-000A-000000000006");
    private static readonly Guid UnitTonId  = new("00000000-0000-0000-000A-000000000007");

    // ══════════════════════════════════════════════════════════════════════════

    public async Task SeedAsync(IServiceProvider services)
    {
        var logger = services.GetService<ILogger<UcmsDbContextSeed>>();
        var policy = CreatePolicy(logger!, nameof(UcmsDbContextSeed));

        await policy.ExecuteAsync(async () =>
        {
            var db          = services.GetRequiredService<UcmsDbContext>();
            var userManager = services.GetRequiredService<UserManager<User>>();
            var roleManager = services.GetRequiredService<RoleManager<Role>>();

            // 1. Asosiy spravochniklar
            await SeedRolesAsync(roleManager, logger);
            await SeedMeasurementUnitsAsync(db, logger);

            // 2. OWNER tashkilot va foydalanuvchisi
            await SeedOwnerOrgAsync(db, logger);
            await SeedOwnerUsersAsync(userManager, logger);
            await SeedOwnerEmployeeAsync(db, userManager, logger);

            // 3. TENANT 1 — to'liq ma'lumotlar bilan
            await SeedTenant1OrgAsync(db, logger);
            await SeedTenant1UsersAsync(userManager, logger);
            await SeedTenant1ProjectsAsync(db, logger);
            await SeedTenant1BrigadesAsync(db, logger);
            await SeedTenant1WorkLogsAsync(db, logger);
            await SeedTenant1FinanceAsync(db, logger);

            // 4. IXTIYOR — shaxsiy qurilish pudratchisi tashkiloti
            await SeedIhtiyorOrgAsync(db, logger);
            await SeedIhtiyorUsersAsync(userManager, logger);
            await SeedIhtiyorEmployeesAsync(db, userManager, logger);
            await SeedIhtiyorProjectAsync(db, logger);

        });
    }

    // ══════════════════════════════════════════════════════════════════════════
    // ROLLAR
    // ══════════════════════════════════════════════════════════════════════════

    private static async Task SeedRolesAsync(RoleManager<Role> roleManager, ILogger? logger)
    {
        var roles = new[]
        {
            new Role { Id = AdminRoleId,     Name = "Admin",      NormalizedName = "ADMIN",
                       Description = "Tizim administratori — to'liq huquq" },
            new Role { Id = ManagerRoleId,   Name = "Manager",    NormalizedName = "MANAGER",
                       Description = "Loyiha menejeri — loyiha va smeta boshqaruvi" },
            new Role { Id = BrigadirRoleId,  Name = "Brigadir",   NormalizedName = "BRIGADIR",
                       Description = "Brigada boshlig'i — ish jurnaliga yozish" },
            new Role { Id = AccountantRoleId, Name = "Accountant", NormalizedName = "ACCOUNTANT",
                       Description = "Hisobchi — to'lov va aktlar boshqaruvi" },
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role.Name!))
            {
                var result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                    logger?.LogInformation("[Seed] Rol: {Role}", role.Name);
                else
                    logger?.LogError("[Seed] Rol xato ({Role}): {Errors}",
                        role.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // O'LCHOV BIRLIKLARI
    // ══════════════════════════════════════════════════════════════════════════

    private static async Task SeedMeasurementUnitsAsync(UcmsDbContext db, ILogger? logger)
    {
        if (await db.MeasurementUnits.AnyAsync())
            return;

        var units = new List<MeasurementUnit>
        {
            new() { Id = UnitM2Id,   Code = "M2",   Name = "m²",    NameRu = "м²",   NameEn = "m²",
                    Multiplier = 1,    Type = MeasurementUnitType.Volume,   IsDeleted = false },
            new() { Id = UnitM3Id,   Code = "M3",   Name = "m³",    NameRu = "м³",   NameEn = "m³",
                    Multiplier = 1,    Type = MeasurementUnitType.Volume,   IsDeleted = false },
            new() { Id = UnitMId,    Code = "M",    Name = "m",     NameRu = "м",    NameEn = "m",
                    Multiplier = 1,    Type = MeasurementUnitType.Length,   IsDeleted = false },
            new() { Id = UnitMpId,   Code = "MP",   Name = "m.p.",  NameRu = "м.п.", NameEn = "lm",
                    Multiplier = 1,    Type = MeasurementUnitType.Length,   IsDeleted = false },
            new() { Id = UnitDonaId, Code = "DONA", Name = "dona",  NameRu = "шт.",  NameEn = "pcs",
                    Multiplier = 1,    Type = MeasurementUnitType.Quantity, IsDeleted = false },
            new() { Id = UnitKgId,   Code = "KG",   Name = "kg",    NameRu = "кг",   NameEn = "kg",
                    Multiplier = 1,    Type = MeasurementUnitType.Weight,   IsDeleted = false },
            new() { Id = UnitTonId,  Code = "TON",  Name = "tonna", NameRu = "тонн", NameEn = "ton",
                    Multiplier = 1000, Type = MeasurementUnitType.Weight,   IsDeleted = false },
        };

        await db.MeasurementUnits.AddRangeAsync(units);
        await db.SaveChangesAsync();
        logger?.LogInformation("[Seed] {N} ta o'lchov birligi", units.Count);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // OWNER TASHKILOT
    // ══════════════════════════════════════════════════════════════════════════

    private static async Task SeedOwnerOrgAsync(UcmsDbContext db, ILogger? logger)
    {
        if (await db.Organizations.AnyAsync(o => o.Id == OwnerOrgId))
            return;

        var now = Now();
        await db.Organizations.AddAsync(new Organization
        {
            Id        = OwnerOrgId,
            Name      = "UCMS — Tizim Egasi",
            TaxId     = "0000000000",
            Address   = "Toshkent shahri, Mirzo Ulug'bek tumani",
            Phone     = "+998712345600",
            Email     = "system@ucms.uz",
            Type      = OrganizationType.Owner,
            IsDeleted = false,
            CreatedAt = now, UpdatedAt = now,
            CreatedBy = SysAdminId, UpdatedBy = SysAdminId,
        });
        await db.SaveChangesAsync();
        logger?.LogInformation("[Seed] OWNER tashkilot yaratildi");
    }

    private static async Task SeedOwnerUsersAsync(UserManager<User> um, ILogger? logger)
    {
        // sysadmin — barcha tashkilotlarga to'liq kirish (Owner org + Admin rol)
        await CreateUserAsync(um, logger, new User
        {
            Id                 = SysAdminId,
            UserName           = "sysadmin",
            NormalizedUserName = "SYSADMIN",
            Email              = "sysadmin@ucms.uz",
            NormalizedEmail    = "SYSADMIN@UCMS.UZ",
            EmailConfirmed     = true,
            FullName           = "Tizim Super Admini",
            OrganizationId     = OwnerOrgId,
            IsDeleted          = false,
            CreatedAt          = Now(), UpdatedAt = Now(),
            CreatedBy          = SysAdminId, UpdatedBy = SysAdminId,
        }, "SysAdmin123!", "Admin");
    }

    /// <summary>
    /// OWNER tashkiloti uchun brigada va sysadminga bog'langan Employee yozuvi.
    /// Bu sysadminni "xodim" sifatida ham tizimda ko'rinishini ta'minlaydi
    /// (Employee/Brigade modullarida OWNER tashkiloti bo'sh ko'rinmasligi uchun).
    /// </summary>
    private static async Task SeedOwnerEmployeeAsync(UcmsDbContext db, UserManager<User> um, ILogger? logger)
    {
        if (await db.Employees.AnyAsync(e => e.Id == OwnerEmployeeId))
            return;

        var now = Now();

        await db.Brigades.AddAsync(new Brigade
        {
            Id             = OwnerBrigadeId,
            OrganizationId = OwnerOrgId,
            Name           = "Boshqaruv brigadasi",
            ForemanName    = "Tizim Super Admini",
            IsActive       = true, IsDeleted = false,
            CreatedAt      = now, UpdatedAt = now,
            CreatedBy      = SysAdminId, UpdatedBy = SysAdminId,
        });
        await db.SaveChangesAsync();

        await db.Employees.AddAsync(new Employee
        {
            Id             = OwnerEmployeeId,
            OrganizationId = OwnerOrgId,
            Name           = "Tizim Super Admini",
            Position       = "Super Admin",
            UserId         = SysAdminId,
            BrigadeId      = OwnerBrigadeId,
            IsActive       = true, IsDeleted = false,
            CreatedAt      = now, UpdatedAt = now,
            CreatedBy      = SysAdminId, UpdatedBy = SysAdminId,
        });
        await db.SaveChangesAsync();

        // sysadmin User yozuvini Employee bilan bog'lash (teskari havola)
        var sysadmin = await um.FindByIdAsync(SysAdminId.ToString());
        if (sysadmin is not null)
        {
            sysadmin.EmployeeId = OwnerEmployeeId;
            await um.UpdateAsync(sysadmin);
        }

        logger?.LogInformation("[Seed] OWNER: brigada va sysadmin uchun Employee yaratildi");
    }

    // ══════════════════════════════════════════════════════════════════════════
    // TENANT 1 — Ihtiyor Qurilish Kompaniyasi
    // ══════════════════════════════════════════════════════════════════════════

    private static async Task SeedTenant1OrgAsync(UcmsDbContext db, ILogger? logger)
    {
        if (await db.Organizations.AnyAsync(o => o.Id == T1OrgId))
            return;

        var now = Now();
        await db.Organizations.AddAsync(new Organization
        {
            Id        = T1OrgId,
            Name      = "Ihtiyor Qurilish Kompaniyasi",
            TaxId     = "1234567890",
            Address   = "Toshkent shahri, Chilonzor tumani, 9-mavze",
            Phone     = "+998712345678",
            Email     = "info@demo-qurilish.uz",
            Type      = OrganizationType.Tenant,
            IsTest    = true,
            IsDeleted = false,
            CreatedAt = now, UpdatedAt = now,
            CreatedBy = T1AdminId, UpdatedBy = T1AdminId,
        });
        await db.SaveChangesAsync();
        logger?.LogInformation("[Seed] TENANT-1 tashkilot yaratildi");
    }

    private static async Task SeedTenant1UsersAsync(UserManager<User> um, ILogger? logger)
    {
        var now = Now();

        // admin — T1 tashkiloti administratori
        await CreateUserAsync(um, logger, new User
        {
            Id = T1AdminId, UserName = "admin", NormalizedUserName = "ADMIN",
            Email = "admin@demo-qurilish.uz", NormalizedEmail = "ADMIN@DEMO-QURILISH.UZ",
            EmailConfirmed = true, FullName = "Ahmadov Bahodir Umarovich",
            OrganizationId = T1OrgId, IsDeleted = false,
            CreatedAt = now, UpdatedAt = now, CreatedBy = T1AdminId, UpdatedBy = T1AdminId,
        }, "Admin123!", "Admin");

        // manager — loyiha menejeri
        await CreateUserAsync(um, logger, new User
        {
            Id = T1ManagerId, UserName = "manager", NormalizedUserName = "MANAGER",
            Email = "manager@demo-qurilish.uz", NormalizedEmail = "MANAGER@DEMO-QURILISH.UZ",
            EmailConfirmed = true, FullName = "Ergashev Jahongir Saidovich",
            OrganizationId = T1OrgId, IsDeleted = false,
            CreatedAt = now, UpdatedAt = now, CreatedBy = T1AdminId, UpdatedBy = T1AdminId,
        }, "Manager123!", "Manager");

        // brigadir — ish jurnaliga yozadi
        await CreateUserAsync(um, logger, new User
        {
            Id = T1BrigadirId, UserName = "brigadir", NormalizedUserName = "BRIGADIR",
            Email = "brigadir@demo-qurilish.uz", NormalizedEmail = "BRIGADIR@DEMO-QURILISH.UZ",
            EmailConfirmed = true, FullName = "Toshmatov Sherzod Hasanovich",
            OrganizationId = T1OrgId, IsDeleted = false,
            CreatedAt = now, UpdatedAt = now, CreatedBy = T1AdminId, UpdatedBy = T1AdminId,
        }, "Brigadir123!", "Brigadir");

        // accountant — to'lov va aktlar
        await CreateUserAsync(um, logger, new User
        {
            Id = T1AccountantId, UserName = "accountant", NormalizedUserName = "ACCOUNTANT",
            Email = "accountant@demo-qurilish.uz", NormalizedEmail = "ACCOUNTANT@DEMO-QURILISH.UZ",
            EmailConfirmed = true, FullName = "Nazarova Gulnora Alijonovna",
            OrganizationId = T1OrgId, IsDeleted = false,
            CreatedAt = now, UpdatedAt = now, CreatedBy = T1AdminId, UpdatedBy = T1AdminId,
        }, "Accountant123!", "Accountant");
    }

    private static async Task SeedTenant1ProjectsAsync(UcmsDbContext db, ILogger? logger)
    {
        if (await db.Projects.AnyAsync(p => p.Id == T1Project1Id))
            return;

        var now = Now();

        // ── Loyiha 1: Faol, to'liq smeta bilan ──────────────────────────────
        var p1 = new Project
        {
            Id             = T1Project1Id,
            OrganizationId = T1OrgId,
            Name           = "Yunusobod-14 — 2-sektsiya ta'mirlash",
            Address        = "Toshkent, Yunusobod tumani, 14-mavze, 3-uy",
            Description    = "Ko'p qavatli turar-joy binosi ichki bezak ishlari",
            ContractNumber = "2024/001",
            ContractDate   = D(2024, 1, 15),
            StartDate      = D(2024, 2,  1),
            EndDate        = D(2024, 9, 30),
            Status         = ProjectStatus.InProgress,
            IsDeleted      = false,
            CreatedAt      = now, UpdatedAt = now,
            CreatedBy      = T1AdminId, UpdatedBy = T1AdminId,
        };

        // Smeta hujjatlari
        var est1 = new Estimate
        {
            Id          = T1P1Est1Id,
            ProjectId   = T1Project1Id,
            Name        = "Asosiy smeta",
            Description = "Ichki bezak ishlari asosiy smeta hujjati",
            Order       = 1,
            IsDeleted   = false,
            CreatedAt   = now, UpdatedAt = now,
            CreatedBy   = T1AdminId, UpdatedBy = T1AdminId,
        };

        // Smeta — bo'limlar
        var s1 = Sec(T1P1Sec1Id, T1P1Est1Id, "Pol ishlari", 1);
        var s2 = Sec(T1P1Sec2Id, T1P1Est1Id, "Devor va shift ishlari", 2);

        var items1 = new[]
        {
            Item(T1P1Item1Id, T1P1Sec1Id, "Pol shtukaturkasi (M-200 beton stяjka)", UnitM2Id,
                 450m, 85_000m, 55_000m, 1),
            Item(T1P1Item2Id, T1P1Sec1Id, "Keramik plitka qo'yish", UnitM2Id,
                 450m, 120_000m, 80_000m, 2),
            Item(T1P1Item3Id, T1P1Sec2Id, "Gips shtukaturka (devorlar)", UnitM2Id,
                 1_200m, 48_000m, 30_000m, 1),
            Item(T1P1Item4Id, T1P1Sec2Id, "Bo'yoq (2 qavat)", UnitM2Id,
                 1_200m, 22_000m, 14_000m, 2),
            Item(T1P1Item5Id, T1P1Sec2Id, "Gips karton (GKL) montaj", UnitM2Id,
                 320m, 75_000m, 50_000m, 3),
        };

        // ── Loyiha 2: Rejalashtirish bosqichi ──────────────────────────────
        var p2 = new Project
        {
            Id             = T1Project2Id,
            OrganizationId = T1OrgId,
            Name           = "Sergeli — ofis binosi ta'mirlash",
            Address        = "Toshkent, Sergeli tumani, 7-mavze",
            Description    = "Ofis binosi to'liq ta'mirlash va jihozlash",
            ContractNumber = "2025/003",
            ContractDate   = D(2025, 3, 1),
            StartDate      = D(2025, 4, 1),
            EndDate        = D(2025, 10, 31),
            Status         = ProjectStatus.Planning,
            IsDeleted      = false,
            CreatedAt      = now, UpdatedAt = now,
            CreatedBy      = T1AdminId, UpdatedBy = T1AdminId,
        };

        var est2 = new Estimate
        {
            Id          = T1P2Est1Id,
            ProjectId   = T1Project2Id,
            Name        = "Asosiy smeta",
            Description = "Ofis binosi ta'mirlash asosiy smeta hujjati",
            Order       = 1,
            IsDeleted   = false,
            CreatedAt   = now, UpdatedAt = now,
            CreatedBy   = T1AdminId, UpdatedBy = T1AdminId,
        };

        var s3    = Sec(T1P2Sec1Id, T1P2Est1Id, "Umumiy ta'mirlash ishlari", 1);
        var items2 = new[]
        {
            Item(T1P2Item1Id, T1P2Sec1Id, "Derazalar almashtirish", UnitDonaId,
                 24m, 850_000m, 600_000m, 1),
            Item(T1P2Item2Id, T1P2Sec1Id, "Eshiklar o'rnatish", UnitDonaId,
                 12m, 1_200_000m, 900_000m, 2),
        };

        await db.Projects.AddRangeAsync(p1, p2);
        await db.SaveChangesAsync();

        await db.Estimates.AddRangeAsync(est1, est2);
        await db.SaveChangesAsync();

        await db.EstimateSections.AddRangeAsync(s1, s2, s3);
        await db.SaveChangesAsync();

        await db.EstimateItems.AddRangeAsync(items1);
        await db.EstimateItems.AddRangeAsync(items2);
        await db.SaveChangesAsync();
        logger?.LogInformation("[Seed] TENANT-1: 2 ta loyiha va smeta yaratildi");
    }

    private static async Task SeedTenant1BrigadesAsync(UcmsDbContext db, ILogger? logger)
    {
        if (await db.Brigades.AnyAsync(b => b.Id == T1Brigade1Id))
            return;

        var now = Now();

        await db.Brigades.AddRangeAsync(
            new Brigade
            {
                Id             = T1Brigade1Id,
                OrganizationId = T1OrgId,
                Name           = "Abdullayev brigada (Pol ishlari)",
                ForemanName    = "Abdullayev Jamshid Shodiyevich",
                Phone          = "+998901234568",
                IsActive       = true, IsDeleted = false,
                CreatedAt      = now, UpdatedAt = now,
                CreatedBy      = T1AdminId, UpdatedBy = T1AdminId,
            },
            new Brigade
            {
                Id             = T1Brigade2Id,
                OrganizationId = T1OrgId,
                Name           = "Karimov brigada (Devor ishlari)",
                ForemanName    = "Karimov Sherzod Aliyevich",
                Phone          = "+998901234569",
                IsActive       = true, IsDeleted = false,
                CreatedAt      = now, UpdatedAt = now,
                CreatedBy      = T1AdminId, UpdatedBy = T1AdminId,
            }
        );
        await db.SaveChangesAsync();
        logger?.LogInformation("[Seed] TENANT-1: 2 ta brigada yaratildi");
    }

    private static async Task SeedTenant1WorkLogsAsync(UcmsDbContext db, ILogger? logger)
    {
        if (await db.WorkLogs.AnyAsync(w => w.Id == T1WL1Id))
            return;

        var now = Now();
        var uid = T1ManagerId;

        // WL1 — Brigade1, pol shtukaturka, Confirmed
        var wl1 = new WorkLog
        {
            Id               = T1WL1Id,
            ProjectId        = T1Project1Id,
            BrigadeId        = T1Brigade1Id,
            EstimateItemId   = T1P1Item1Id,
            Date             = D(2024, 3, 10),
            Volume           = 120m,
            BrigadeUnitPrice = 55_000m,
            TotalAmount      = 120m * 55_000m,   // 6,600,000
            Status           = WorkLogStatus.Confirmed,
            Note             = null,
            CreatedAt = now, UpdatedAt = now, CreatedBy = uid, UpdatedBy = uid,
        };

        // WL2 — Brigade1, plitka, Confirmed
        var wl2 = new WorkLog
        {
            Id               = T1WL2Id,
            ProjectId        = T1Project1Id,
            BrigadeId        = T1Brigade1Id,
            EstimateItemId   = T1P1Item2Id,
            Date             = D(2024, 3, 25),
            Volume           = 80m,
            BrigadeUnitPrice = 80_000m,
            TotalAmount      = 80m * 80_000m,    // 6,400,000
            Status           = WorkLogStatus.Confirmed,
            CreatedAt = now, UpdatedAt = now, CreatedBy = uid, UpdatedBy = uid,
        };

        // WL3 — Brigade2, gips shtukaturka, Paid (BrigadePaymentId keyinroq SeedTenant1FinanceAsync da o'rnatiladi)
        var wl3 = new WorkLog
        {
            Id               = T1WL3Id,
            ProjectId        = T1Project1Id,
            BrigadeId        = T1Brigade2Id,
            EstimateItemId   = T1P1Item3Id,
            Date             = D(2024, 4, 5),
            Volume           = 300m,
            BrigadeUnitPrice = 30_000m,
            TotalAmount      = 300m * 30_000m,   // 9,000,000
            Status           = WorkLogStatus.Paid,
            CreatedAt = now, UpdatedAt = now, CreatedBy = uid, UpdatedBy = uid,
        };

        // WL4 — Brigade2, bo'yoq, Draft
        var wl4 = new WorkLog
        {
            Id               = T1WL4Id,
            ProjectId        = T1Project1Id,
            BrigadeId        = T1Brigade2Id,
            EstimateItemId   = T1P1Item4Id,
            Date             = D(2024, 4, 20),
            Volume           = 400m,
            BrigadeUnitPrice = 14_000m,
            TotalAmount      = 400m * 14_000m,   // 5,600,000
            Status           = WorkLogStatus.Draft,
            CreatedAt = now, UpdatedAt = now, CreatedBy = T1BrigadirId, UpdatedBy = T1BrigadirId,
        };

        // WL5 — Brigade1, GKL, Rejected
        var wl5 = new WorkLog
        {
            Id               = T1WL5Id,
            ProjectId        = T1Project1Id,
            BrigadeId        = T1Brigade1Id,
            EstimateItemId   = T1P1Item5Id,
            Date             = D(2024, 4, 15),
            Volume           = 50m,
            BrigadeUnitPrice = 50_000m,
            TotalAmount      = 50m * 50_000m,    // 2,500,000
            Status           = WorkLogStatus.Rejected,
            Note             = "Sifat talablariga javob bermaydi — qayta bajarish kerak",
            CreatedAt = now, UpdatedAt = now, CreatedBy = T1BrigadirId, UpdatedBy = uid,
        };

        await db.WorkLogs.AddRangeAsync(wl1, wl2, wl3, wl4, wl5);
        await db.SaveChangesAsync();
        logger?.LogInformation("[Seed] TENANT-1: 5 ta work log yaratildi");
    }

    private static async Task SeedTenant1FinanceAsync(UcmsDbContext db, ILogger? logger)
    {
        if (await db.BrigadePayments.AnyAsync(b => b.Id == T1BP1Id))
            return;

        var now = Now();
        var uid = T1AccountantId;

        // ── BrigadePayment: Brigade2 ga to'lov (WL3 uchun) ──────────────────
        var bp1 = new BrigadePayment
        {
            Id            = T1BP1Id,
            ProjectId     = T1Project1Id,
            BrigadeId     = T1Brigade2Id,
            Date          = D(2024, 4, 10),
            Amount        = 9_000_000m,
            PaymentMethod = PaymentMethod.BankTransfer,
            Note          = "2024-yil mart oyi ish haqi",
            CreatedAt = now, UpdatedAt = now, CreatedBy = uid, UpdatedBy = uid,
        };

        // ── ClientAct ────────────────────────────────────────────────────────
        // Pol shtukaturka + plitka (120m² + 80m²) uchun akt
        var act1 = new ClientAct
        {
            Id          = T1Act1Id,
            ProjectId   = T1Project1Id,
            ActNumber   = "AKT-2024/001",
            ActDate     = D(2024, 4, 1),
            TotalAmount = (120m * 85_000m) + (80m * 120_000m),   // 10,200,000 + 9,600,000 = 19,800,000
            Status      = ActStatus.PaidPartially,
            Note        = "1-oylik ish natijalari bo'yicha akt",
            CreatedAt = now, UpdatedAt = now, CreatedBy = uid, UpdatedBy = uid,
        };

        var actItem1 = new ClientActItem
        {
            Id             = NewId(),
            ActId          = T1Act1Id,
            EstimateItemId = T1P1Item1Id,
            Volume         = 120m,
            UnitPrice      = 85_000m,
            TotalAmount    = 120m * 85_000m,     // 10,200,000
        };

        var actItem2 = new ClientActItem
        {
            Id             = NewId(),
            ActId          = T1Act1Id,
            EstimateItemId = T1P1Item2Id,
            Volume         = 80m,
            UnitPrice      = 120_000m,
            TotalAmount    = 80m * 120_000m,     // 9,600,000
        };

        // ── ClientPayment: qisman to'lov ────────────────────────────────────
        var cp1 = new ClientPayment
        {
            Id            = T1CP1Id,
            ProjectId     = T1Project1Id,
            ActId         = T1Act1Id,
            Date          = D(2024, 4, 15),
            Amount        = 10_000_000m,         // 19.8m dan 10m to'langan → PaidPartially
            PaymentMethod = PaymentMethod.BankTransfer,
            Note          = "Avans to'lov (50%)",
            CreatedAt = now, UpdatedAt = now, CreatedBy = uid, UpdatedBy = uid,
        };

        await db.BrigadePayments.AddAsync(bp1);
        await db.SaveChangesAsync();

        // WL3 ni BrigadePayment ga bog'lash (FK tartib muammosi sababli bu yerda qilinadi)
        var wl3 = await db.WorkLogs.FindAsync(T1WL3Id);
        if (wl3 is not null)
        {
            wl3.BrigadePaymentId = T1BP1Id;
            await db.SaveChangesAsync();
        }

        await db.ClientActs.AddAsync(act1);
        await db.ClientActItems.AddRangeAsync(actItem1, actItem2);
        await db.ClientPayments.AddAsync(cp1);
        await db.SaveChangesAsync();
        logger?.LogInformation("[Seed] TENANT-1: moliyaviy ma'lumotlar yaratildi (akt + to'lovlar)");
    }

    // ══════════════════════════════════════════════════════════════════════════
    // IXTIYOR — shaxsiy qurilish pudratchisi tashkiloti (Daminov Ixtiyor Jonovich)
    // ══════════════════════════════════════════════════════════════════════════

    private static async Task SeedIhtiyorOrgAsync(UcmsDbContext db, ILogger? logger)
    {
        if (await db.Organizations.AnyAsync(o => o.Id == IhtiyorOrgId))
            return;

        var now = Now();
        await db.Organizations.AddAsync(new Organization
        {
            Id        = IhtiyorOrgId,
            Name      = "IXTIYOR PUDRAT",
            TaxId     = "3057891240",
            Address   = "Toshkent shahri, Yashnobod tumani",
            Phone     = "+79015227700",
            Email     = "ixtiyor.pudrat@gmail.com",
            Type      = OrganizationType.Tenant,
            CreatedAt = now, 
            UpdatedAt = now,
            CreatedBy = IhtiyorDirectorUserId, UpdatedBy = IhtiyorDirectorUserId,
        });
        await db.SaveChangesAsync();
        logger?.LogInformation("[Seed] IXTIYOR tashkilot yaratildi");
    }

    private static async Task SeedIhtiyorUsersAsync(UserManager<User> um, ILogger? logger)
    {
        var now = Now();

        // direktor — Daminov Ixtiyor Jonovich, o'z tashkilotida to'liq huquq (Admin rol)
        await CreateUserAsync(um, logger, new User
        {
            Id                 = IhtiyorDirectorUserId,
            UserName           = "ixtiyor.direktor",
            NormalizedUserName = "IXTIYOR.DIREKTOR",
            Email              = "ixtiyor.pudrat@gmail.com",
            NormalizedEmail    = "IXTIYOR.PUDRAT@GMAIL.COM",
            EmailConfirmed     = true,
            FullName           = "Daminov Ixtiyor Jonovich",
            OrganizationId     = IhtiyorOrgId,
            IsDeleted          = false,
            CreatedAt          = now, UpdatedAt = now,
            CreatedBy          = IhtiyorDirectorUserId, UpdatedBy = IhtiyorDirectorUserId,
        }, "Ixtiyor123!", "Admin");
    }

    /// <summary>
    /// IXTIYOR tashkiloti uchun: direktor Employee (Daminov Ixtiyor) + 1 ta brigada
    /// va unga biriktirilgan ishchi Employee (shuvoqchi), shu ishchi ayni paytda brigada boshlig'i.
    /// </summary>
    private static async Task SeedIhtiyorEmployeesAsync(UcmsDbContext db, UserManager<User> um, ILogger? logger)
    {
        if (await db.Employees.AnyAsync(e => e.Id == IhtiyorDirectorEmpId))
            return;

        var now = Now();
        const string foremanName = "Yusupov Aziz Tursunovich";

        await db.Brigades.AddAsync(new Brigade
        {
            Id             = IhtiyorBrigadeId,
            OrganizationId = IhtiyorOrgId,
            Name           = "Yusupov brigada (Shuvoq ishlari)",
            ForemanName    = foremanName,
            Phone          = "+998904445566",
            IsActive       = true, IsDeleted = false,
            CreatedAt      = now, UpdatedAt = now,
            CreatedBy      = IhtiyorDirectorUserId, UpdatedBy = IhtiyorDirectorUserId,
        });
        await db.SaveChangesAsync();

        await db.Employees.AddRangeAsync(
            // Direktor — Daminov Ixtiyor, User bilan bog'langan
            new Employee
            {
                Id             = IhtiyorDirectorEmpId,
                OrganizationId = IhtiyorOrgId,
                Name           = "Daminov Ixtiyor Jonovich",
                Position       = "Direktor",
                UserId         = IhtiyorDirectorUserId,
                BrigadeId      = null,
                IsActive       = true,
                CreatedAt      = now, 
                UpdatedAt = now,
                CreatedBy      = IhtiyorDirectorUserId, 
                UpdatedBy = IhtiyorDirectorUserId,
            },
            // Ishchi — shuvoqchi, ayni paytda brigada boshlig'i (alohida User talab qilinmaydi)
            new Employee
            {
                Id             = IhtiyorWorkerEmpId,
                OrganizationId = IhtiyorOrgId,
                Name           = foremanName,
                Position       = "Shtukatorchi (shuvoqchi) / Brigada boshlig'i",
                UserId         = null,
                BrigadeId      = IhtiyorBrigadeId,
                IsActive       = true,
                CreatedAt      = now, 
                UpdatedAt = now,
                CreatedBy      = IhtiyorDirectorUserId, 
                UpdatedBy = IhtiyorDirectorUserId,
            }
        );
        await db.SaveChangesAsync();

        // direktor User yozuvini Employee bilan bog'lash (teskari havola)
        var director = await um.FindByIdAsync(IhtiyorDirectorUserId.ToString());
        if (director is not null)
        {
            director.EmployeeId = IhtiyorDirectorEmpId;
            await um.UpdateAsync(director);
        }

        logger?.LogInformation("[Seed] IXTIYOR: brigada, direktor va ishchi Employee yaratildi");
    }

    /// <summary>
    /// IXTIYOR tashkiloti uchun 1 ta loyiha va smeta — real smeta fayllaridan olingan
    /// namuna ma'lumotlar asosida ("Смета ИКС отделка сек. 2,3 финал" — Zakazchik narxi,
    /// "Новая таблица 2" — Ixtiyor o'z ishchilariga to'laydigan narx).
    /// </summary>
    private static async Task SeedIhtiyorProjectAsync(UcmsDbContext db, ILogger? logger)
    {
        if (await db.Projects.AnyAsync(p => p.Id == IhtiyorProjectId))
            return;

        var now = Now();

        var project = new Project
        {
            Id             = IhtiyorProjectId,
            OrganizationId = IhtiyorOrgId,
            Name           = "Pivchenkova ko'chasi, 14-uy — 2,3-sektsiya otdelka",
            Address        = "Moskva sh., G'arbiy ma.okrugi, Fili-Davydkovo tumani, Pivchenkova ko'chasi, 14-uy",
            Description    = "Tipik qavatlarni otdelka qilish bo'yicha pudrat ishlari kompleksi (2,3-sektsiyalar)",
            ClientName     = "OOO \"IKS\"",
            ContractNumber = "2026/2-3",
            ContractDate   = D(2026, 1, 10),
            ContractValue  = 16_061_329.00m,
            StartDate      = D(2026, 1, 15),
            EndDate        = D(2026, 8, 31),
            Status         = ProjectStatus.InProgress,
            IsDeleted      = false,
            CreatedAt      = now, UpdatedAt = now,
            CreatedBy      = IhtiyorDirectorUserId, UpdatedBy = IhtiyorDirectorUserId,
        };

        var est = new Estimate
        {
            Id          = IhtiyorEstId,
            ProjectId   = IhtiyorProjectId,
            Name        = "Smeta kontrakti (2,3-sektsiya otdelka)",
            Description = "Zakazchik (OOO IKS) bilan tuzilgan shartnoma narxining tafsilnomasi",
            Order       = 1,
            IsDeleted   = false,
            CreatedAt   = now, UpdatedAt = now,
            CreatedBy   = IhtiyorDirectorUserId, UpdatedBy = IhtiyorDirectorUserId,
        };

        var sec1 = Sec(IhtiyorSec1Id, IhtiyorEstId, "Pol ishlari", 1);
        var sec2 = Sec(IhtiyorSec2Id, IhtiyorEstId, "Devor ishlari", 2);

        var items = new[]
        {
            // ── Pol ishlari ──
            Item(IhtiyorItem1Id, IhtiyorSec1Id,
                 "Ajratuvchi qatlam — polietilen plyonka T 0,200 (1 sort, 1 qavat)", UnitM2Id,
                 2360.53m, 56.57m, 25m, 1),
            Item(IhtiyorItem2Id, IhtiyorSec1Id,
                 "Yarim quruq sement-qum stяjka M150, fibrоvolokno bilan armirlangan — 84mm", UnitM2Id,
                 2360.53m, 932.29m, 420m, 2),
            Item(IhtiyorItem3Id, IhtiyorSec1Id,
                 "Keramogranit plitka 600x600x10, choklarini fugovka qilish bilan", UnitM2Id,
                 2360.53m, 1195.24m, 650m, 3),
            // ── Devor ishlari ──
            Item(IhtiyorItem4Id, IhtiyorSec2Id,
                 "Gips asosli shtukaturka (tekislash), yaxshilangan — 17mm", UnitM2Id,
                 7980.40m, 733.08m, 350m, 1),
            Item(IhtiyorItem5Id, IhtiyorSec2Id,
                 "Gips asosli shpaklyovka — 2mm", UnitM2Id,
                 7980.40m, 633.48m, 180m, 2),
        };

        await db.Projects.AddAsync(project);
        await db.SaveChangesAsync();

        await db.Estimates.AddAsync(est);
        await db.SaveChangesAsync();

        await db.EstimateSections.AddRangeAsync(sec1, sec2);
        await db.SaveChangesAsync();

        await db.EstimateItems.AddRangeAsync(items);
        await db.SaveChangesAsync();

        logger?.LogInformation("[Seed] IXTIYOR: loyiha va smeta (2 bo'lim, 5 ish turi) yaratildi");
    }

    // ══════════════════════════════════════════════════════════════════════════
    // HELPERS
    // ══════════════════════════════════════════════════════════════════════════

    private static async Task CreateUserAsync(
        UserManager<User> um,
        ILogger? logger,
        User user,
        string password,
        string role)
    {
        if (await um.FindByNameAsync(user.UserName!) is not null)
            return;

        var result = await um.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            logger?.LogError("[Seed] Foydalanuvchi yaratishda xato ({User}): {Errors}",
                user.UserName, string.Join(", ", result.Errors.Select(e => e.Description)));
            return;
        }

        await um.AddToRoleAsync(user, role);
        logger?.LogInformation("[Seed] User: {User} / {Pwd} [{Role}]", user.UserName, password, role);
    }

    private static EstimateSection Sec(Guid id, Guid estimateId, string name, int order)
    {
        return new() { Id = id, EstimateId = estimateId, Name = name, Order = order };
    }

    private static EstimateItem Item(
        Guid id, Guid sectionId, string name, Guid measurementUnitId,
        decimal volume, decimal clientPrice, decimal brigadePrice, int order)
    {
        return new()
        {
            Id                = id,
            SectionId         = sectionId,
            Name              = name,
            MeasurementUnitId = measurementUnitId,
            Volume            = volume,
            ClientUnitPrice   = clientPrice,
            BrigadeUnitPrice  = brigadePrice,
            Order             = order,
        };
    }

    private static DateTimeOffset D(int y, int m, int d)
    {
        return new(y, m, d, 0, 0, 0, TimeSpan.Zero);
    }

    private static DateTimeOffset Now()
    {
        return DateTimeOffset.UtcNow;
    }

    private static Guid NewId()
    {
        return Guid.NewGuid();
    }

    private static AsyncRetryPolicy CreatePolicy(ILogger<UcmsDbContextSeed> logger, string prefix, int retries = 3)
    {
        return Policy.Handle<NpgsqlException>().WaitAndRetryAsync(
            retryCount: retries,
            sleepDurationProvider: _ => TimeSpan.FromSeconds(5),
            onRetry: (exception, _, retry, _) =>
            {
                logger.LogWarning(exception,
                    "[{Prefix}] Urinish {Retry}/{Retries} — {Message}",
                    prefix, retry, retries, exception.Message);
            });
    }
}
