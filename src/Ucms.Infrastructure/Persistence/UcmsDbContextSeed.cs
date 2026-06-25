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

    // ── Ish turlari / spravochnik ────────────────────────────────────────────
    // Demo smeta itemlari uchun PDF dagi ish turlariga (0012-*) aliaslar.
    // Alohida demo ish turlari yaratilmaydi — faqat PDF dagilar ishlatiladi.
    private static readonly Guid WorkTypeFloorScreedId      = new("00000000-0000-0000-0012-000000000002"); // Полусухая стяжка М150 — 84мм
    private static readonly Guid WorkTypeTileLayingId       = new("00000000-0000-0000-0012-000000000009"); // Керамогранит 600х600х10
    private static readonly Guid WorkTypeWallPlasterId      = new("00000000-0000-0000-0012-000000000018"); // Штукатурка гипс. улучш. 17мм
    private static readonly Guid WorkTypePaintingId         = new("00000000-0000-0000-0012-00000000001D"); // Окраска воднодисперс. 1 слой
    private static readonly Guid WorkTypeDrywallId          = new("00000000-0000-0000-0012-000000000022"); // Фальш стена из ГКЛВ
    private static readonly Guid WorkTypeWindowsId          = new("00000000-0000-0000-0012-000000000031"); // Установка металлических дверей
    private static readonly Guid WorkTypeDoorsId            = new("00000000-0000-0000-0012-000000000032"); // Установка межкомнатных дверей
    private static readonly Guid WorkTypeVaporBarrierId     = new("00000000-0000-0000-0012-000000000001"); // Разделительный слой плёнка
    private static readonly Guid WorkTypeSemiDryScreedId    = new("00000000-0000-0000-0012-000000000002"); // Полусухая стяжка М150 — 84мм
    private static readonly Guid WorkTypePorcelainTileId    = new("00000000-0000-0000-0012-000000000009"); // Керамогранит 600х600х10
    private static readonly Guid WorkTypeGypsumPlasterId    = new("00000000-0000-0000-0012-000000000018"); // Штукатурка гипс. улучш. 17мм
    private static readonly Guid WorkTypeGypsumPuttyId      = new("00000000-0000-0000-0012-00000000001A"); // Шпатлевка гипс. 2мм

    // ── Omborlar / Kassalar / Postavshiklar / Ishlab chiqaruvchilar (fixed) ────
    private static readonly Guid T1MainStockId        = new("00000000-0000-0000-000C-000000000001");
    private static readonly Guid T1CashAccountCashId  = new("00000000-0000-0000-000D-000000000001");
    private static readonly Guid T1CashAccountBankId  = new("00000000-0000-0000-000D-000000000002");
    private static readonly Guid Supplier1Id          = new("00000000-0000-0000-000E-000000000001");
    private static readonly Guid Manufacturer1Id      = new("00000000-0000-0000-000F-000000000001");

    private static readonly Guid IhtiyorStockId           = new("00000000-0000-0000-000C-000000000002");
    private static readonly Guid IhtiyorCashAccountCashId = new("00000000-0000-0000-000D-000000000003");
    private static readonly Guid IhtiyorCashAccountBankId = new("00000000-0000-0000-000D-000000000004");

    // ── Mahsulotlar / Продукты (qurilishda eng ko'p ishlatiladigan 5 ta, fixed) ─
    private static readonly Guid ProductCementId = new("00000000-0000-0000-0010-000000000001");
    private static readonly Guid ProductBrickId  = new("00000000-0000-0000-0010-000000000002");
    private static readonly Guid ProductRebarId  = new("00000000-0000-0000-0010-000000000003");
    private static readonly Guid ProductTileId   = new("00000000-0000-0000-0010-000000000004");
    private static readonly Guid ProductPaintId  = new("00000000-0000-0000-0010-000000000005");

    private static readonly Guid SkuCementId = new("00000000-0000-0000-0011-000000000001");
    private static readonly Guid SkuBrickId  = new("00000000-0000-0000-0011-000000000002");
    private static readonly Guid SkuRebarId  = new("00000000-0000-0000-0011-000000000003");
    private static readonly Guid SkuTileId   = new("00000000-0000-0000-0011-000000000004");
    private static readonly Guid SkuPaintId  = new("00000000-0000-0000-0011-000000000005");

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
            await SeedWorkTypesAsync(db, logger);

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
            await SeedTenant1ReferenceDataAsync(db, logger);

            // 4. IXTIYOR — shaxsiy qurilish pudratchisi tashkiloti
            await SeedIhtiyorOrgAsync(db, logger);
            await SeedIhtiyorUsersAsync(userManager, logger);
            await SeedIhtiyorEmployeesAsync(db, userManager, logger);
            await SeedIhtiyorProjectAsync(db, logger);
            await SeedIhtiyorReferenceDataAsync(db, logger);

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
    // ISH TURLARI (smeta pozitsiyalari uchun sprvashnik)
    // ══════════════════════════════════════════════════════════════════════════

    private static async Task SeedWorkTypesAsync(UcmsDbContext db, ILogger? logger)
    {
        var workTypes = new List<WorkType>
        {
            // ── Pollar (Полы) ──────────────────────────────────────────────────
            new() { Id = new("00000000-0000-0000-0012-000000000001"),
                    Name = "Ajratuvchi qatlam — polietilen plyonka T, 0,200, 1-sort, 1 qavat",
                    NameRu = "Разделительный слой из пленки полиэтиленовый Т, 0,200 1 сорт - 1 слой", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000002"),
                    Name = "Yarim quruq sement-qum stяjka M150, fibrоvolokno bilan armirlangan — 84mm",
                    NameRu = "Полусухая цементно-песчанная стяжка М 150 армированная фиброволокном - 84мм", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000003"),
                    Name = "Yarim quruq sement-qum stяjka M150, fibrоvolokno bilan armirlangan — 85mm",
                    NameRu = "Полусухая цементно-песчанная стяжка М 150 армированная фиброволокном - 85мм", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000004"),
                    Name = "Yarim quruq sement-qum stяjka M150, fibrоvolokno bilan armirlangan — 86mm",
                    NameRu = "Полусухая цементно-песчанная стяжка М 150 армированная фиброволокном - 86мм", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000005"),
                    Name = "Yarim quruq sement-qum stяjka M150, fibrоvolokno bilan armirlangan — 88mm",
                    NameRu = "Полусухая цементно-песчанная стяжка М 150 армированная фиброволокном - 88мм", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000006"),
                    Name = "Yarim quruq sement-qum stяjka M150, fibrоvolokno bilan armirlangan — 18mm",
                    NameRu = "Полусухая цементно-песчанная стяжка М 150 армированная фиброволокном - 18мм", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000007"),
                    Name = "Chuqur singdiruvchi grunt — 1 qavat",
                    NameRu = "Грунтовка глубокого проникновения - 1 слой", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000008"),
                    Name = "Plitka yelimi, fuga",
                    NameRu = "Плиточный клей, затирка", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000009"),
                    Name = "Keramogranit plitka 600x600x10, choklarini fugovka qilish bilan",
                    NameRu = "Керамогранитная плитка 600х600х10 с затиркой межплиточных швов", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-00000000000A"),
                    Name = "Keramogranit plitka 300x300x10, choklarini fugovka qilish bilan",
                    NameRu = "Керамогранитная плитка 300х300х10 с затиркой межплиточных швов", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-00000000000B"),
                    Name = "Sovuqqa chidamli keramogranit plitka 300x300x10, choklarini fugovka qilish bilan",
                    NameRu = "Керамогранитная морозостойкая плитка 300х300х10 с затиркой межплиточных швов", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-00000000000C"),
                    Name = "TEPOFOL EPP issiqlik va tovush izolyatsiyasini o'rnatish, pol tayyorlash bilan (chiqindilarni tozalash, notekisliklarni silliqlash, armaturani kesish, chuqurchalarni ta'mirlash aralashmalari bilan to'ldirish)",
                    NameRu = "Устройство тепло и звукоизоляции из ТЕПОФОЛ ЭПП, с подготовкой полов (уборка мусора, шлифовка неровностей, срезка арматуры, заливка ямы ремонтными составами)", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-00000000000D"),
                    Name = "Laminat ostiga ekstrudirlangan penopolistiroldan list ko'rinishidagi taglik — 3 mm",
                    NameRu = "Подложка листовая под ламинат из экструдированного пенополистирола - 3 мм", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-00000000000E"),
                    Name = "Laminat poldan tashkil qilish, 32-sinf — 8 mm",
                    NameRu = "Устройство полов из ламината, класс 32 - 8 мм", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-00000000000F"),
                    Name = "Ekstrudirlangan penopolistirol λ=0,034 Vt/mK — 200 mm",
                    NameRu = "Экструдированный пенополистирол λ=0,034 Вт/мК - 200 мм", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000010"),
                    Name = "Betonkontakt grunt — 1 qavat",
                    NameRu = "Грунтовка Бетонконтакт - 1 слой", IsDeleted = false },

            // ── Plintuslar / profillar ───────────────────────────────────────────
            new() { Id = new("00000000-0000-0000-0012-000000000011"),
                    Name = "PVX pol plintusi kabel kanali bilan, Hedson, 55x23x2200 mm (Mehmonxona, yotoqxona)",
                    NameRu = "Плинтус ПВХ напольный с кабель каналом Хедсон, 55х23х2200 мм (Гостиная, спальня)", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000012"),
                    Name = "PVX pol plintusi kabel kanali bilan, Hedson, 55x23x2200 mm (Oshxona, holl)",
                    NameRu = "Плинтус ПВХ напольный с кабель каналом Хедсон, 55х23х2200 мм (Кухня, Холл)", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000013"),
                    Name = "Keramogranit plitadan plintus 400x100x7 mm (Lodjiya)",
                    NameRu = "Плинтус из керамогранитной плиты 400х100х7 мм (Лоджия)", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000014"),
                    Name = "Keramogranit plitadan plintus 600x95x10 mm (Lift holli, kvartiralararo dahliz, tambur shlyuz)",
                    NameRu = "Плинтус из керамогранитной плиты 600х95х10 мм (Лиф. холл, межкв. коридор, тамбур шлюз)", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000015"),
                    Name = "Keramogranit plitadan plintus 300x57x7 mm (Chiqindi chiqarish xonasi)",
                    NameRu = "Плинтус из керамогранитной плиты 300х57х7 мм (Помещение мусороудаления)", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000016"),
                    Name = "T-shakldagi ulagich alyuminiy profil 20x1800 (holl bilan mehmonxona va yotoqxona ulanish joyi)",
                    NameRu = "Т-образный стыковочный алюминиевый профиль 20х1800 (стык холла со гостиной и спальней)", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000017"),
                    Name = "Zinapoyalarda kaloshnitsa 100x50 mm",
                    NameRu = "Калошница на лестницах 100х50 мм", IsDeleted = false },

            // ── Devorlar (Штукатурка, Шпатлевка, Обои) ───────────────────────────
            new() { Id = new("00000000-0000-0000-0012-000000000018"),
                    Name = "Gips asosli shtukaturka (tekislash), yaxshilangan — 17 mm",
                    NameRu = "Штукатурка (выравнивание) на гипсовой основе улучшенной - 17 мм", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000019"),
                    Name = "Gips asosli shtukaturka (tekislash), yaxshilangan — 10 mm",
                    NameRu = "Штукатурка (выравнивание) на гипсовой основе улучшенной - 10 мм", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-00000000001A"),
                    Name = "Gips asosli shpaklyovka — 2 mm",
                    NameRu = "Шпатлевка на гипсовой основе - 2 мм", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-00000000001B"),
                    Name = "Flizelin devor qog'ozlari uchun yelim",
                    NameRu = "Клей для флизелиновых обоев", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-00000000001C"),
                    Name = "Flizelin asosli devor qog'ozini yelimlash — 2 mm",
                    NameRu = "Оклейка обоев на флизелиновой основе - 2 мм", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-00000000001D"),
                    Name = "Suv-dispersion, polivinilatsetat bo'yoq bilan bo'yash (yaxshilangan) — 1 qavat",
                    NameRu = "Окраска (улучшенной) воднодисперсионной, поливинилацетатной краской - 1 слой", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-00000000001E"),
                    Name = "Sement-qum aralashmalari bilan to'liq tekislash (yaxshilangan) — 14 mm",
                    NameRu = "Сплошное выравнивание цементно-песчаными смесями, улучшенной - 14 мм", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-00000000001F"),
                    Name = "Sement-qum aralashmalari bilan to'liq tekislash (yaxshilangan) — 17 mm (montaj devor ustidan)",
                    NameRu = "Сплошное выравнивание цементно-песчаными смесями, улучшенной - 17 мм (по мон.)", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000020"),
                    Name = "Sement-qum aralashmalari bilan to'liq tekislash (yaxshilangan) — 8 mm",
                    NameRu = "Сплошное выравнивание цементно-песчаными смесями, улучшенной - 8 мм", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000021"),
                    Name = "Sement-qum aralashmalari bilan to'liq tekislash (yaxshilangan, qalinligi ko'rsatilmagan)",
                    NameRu = "Сплошное выравнивание цементно-песчаными смесями, улучшенный (без указ. толщины)", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000022"),
                    Name = "Karkas bo'yicha GKLV (namlikka chidamli gipskarton)dan soxta devor yasash",
                    NameRu = "Устройство фальш стены из ГКЛВ по каркасу", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000023"),
                    Name = "Devor yuzasi bo'ylab sement asosli shpaklyovka, GKL choklariga armirlovchi lenta yopishtirilgan holda — 2 mm",
                    NameRu = "Шпатлевка на цементном основе по всей поверхности стен с проклейкой армирующей ленты по швам стыка ГКЛ - 2 мм", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000024"),
                    Name = "Bir komponentli elastik polimer gidroizolyatsiya, burchaklarda choklarni germetiklash uchun suv o'tkazmaydigan lenta bilan — 2 qavat",
                    NameRu = "Однокомпонентная полимерная эластичная гидроизоляция с прокладкой по углам водонепроницаемой ленты для герметизации швов - 2 слоя", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000025"),
                    Name = "Keramogranit plitka 200x300x7 (oq), choklarini fugovka qilish bilan",
                    NameRu = "Керамогранитная плитка 200х300х7 (белая) с затиркой межплиточных швов", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000026"),
                    Name = "Sement-qum aralashmalari bilan shtukaturka (yaxshilangan) — 17 mm (g'isht devor ustidan)",
                    NameRu = "Штукатурка цементно-песчаными смесями, улучшенной - 17 мм (по кладке)", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000027"),
                    Name = "Sement-qum aralashmalari bilan shtukaturka (yaxshilangan) — 14 mm",
                    NameRu = "Штукатурка цементно-песчаными смесями, улучшенной - 14 мм", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000028"),
                    Name = "Sement asosli shpaklyovka — 2 mm",
                    NameRu = "Шпатлевка на цементном основе - 2 мм", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000029"),
                    Name = "Fasad uchun atmosfera ta'siriga chidamli G1 bo'yoq bilan bo'yash (yaxshilangan) — 1 qavat",
                    NameRu = "Окраска (улучшенной) фасадной, атмосферостойкой краской Г1 - 1 слой", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-00000000002A"),
                    Name = "Fakturali (teksturali) G1 bo'yoq bilan bo'yash (yaxshilangan) — 1 qavat",
                    NameRu = "Окраска (улучшенной) фактурной (текстурой) краской Г1 - 1 слой", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-00000000002B"),
                    Name = "Suv-emulsion, namlikka chidamli G1 bo'yoq bilan bo'yash (oddiy) — 1 qavat",
                    NameRu = "Окраска (простой) водоэмульсионный, влагостойкой краской Г1 - 1 слой", IsDeleted = false },

            // ── Otkoslar, eshiklar ────────────────────────────────────────────────
            new() { Id = new("00000000-0000-0000-0012-00000000002C"),
                    Name = "Deraza va eshik otkoslarini Betonkontakt bilan grunlash — 1 qavat",
                    NameRu = "Грунтовка Бетонконтактом оконных и дверных откосов - 1 слой", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-00000000002D"),
                    Name = "Otkoslarni sement-qum aralashmalari bilan to'r bo'yicha shtukaturka qilish",
                    NameRu = "Штукатурка откосов цементно-песчаными смесями, по сетке", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-00000000002E"),
                    Name = "Deraza va eshik otkoslarini 2 qavat bo'yash",
                    NameRu = "Окраска оконных и дверных откосов в 2 слоя", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-00000000002F"),
                    Name = "Deraza va eshik otkoslarini PVX burchaklar bilan bezatish 20x20 mm",
                    NameRu = "Обрамление оконных и дверных откосов ПВХ уголками 20х20 мм", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000030"),
                    Name = "To'r (ulagichlar va devorlarning ulanish joylari)",
                    NameRu = "Сетка (перемычки и места соединения стен)", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000031"),
                    Name = "Metall eshiklarni o'rnatish (kvartiraning kirish eshiklari)",
                    NameRu = "Установка металлических дверей (входные квартирные двери)", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000032"),
                    Name = "Xonalararo eshiklarni o'rnatish",
                    NameRu = "Установка межкомнатных дверей", IsDeleted = false },

            // ── Fasad ─────────────────────────────────────────────────────────────
            new() { Id = new("00000000-0000-0000-0012-000000000033"),
                    Name = "Universal fasad grunti — 1 qavat",
                    NameRu = "Грунтовка фасадная универсальная - 1 слой", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000034"),
                    Name = "Mineral momiq plitalardan issiqlik izolyatori (Technonikol — TEXNOFAS DEKOR) ρ=100-120 kg/m³, λB=0,041 Vt/m°S, uzilishga R≥12 kPa — 150 mm, izolyatorni yopishtirish uchun yelim (Technonikol 110) — 5 mm",
                    NameRu = "Утеплитель минераловатные плиты (Технониколь — ТЕХНОФАС ДЕКОР) ρ=100-120 кг/м³, λБ=0,041 Вт/м°С, R на отрыв≥12 кПа — 150 мм, клей для приклеивания утеплителя (Технониколь 110) — 5 мм", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000035"),
                    Name = "Shtukaturka-yelim qatlami (Technonikol 210), ishqorga chidamli fasad shisha to'r M3600 bilan armirlangan — 8 mm",
                    NameRu = "Штукатурно-клеевой слой (Технониколь 210), армированный фасадной стеклотканевой щелочестойкой сеткой М3600 - 8 мм", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000036"),
                    Name = "Bazalt tolasi asosidagi mineral momiq plitalardan issiqlik izolyatori ρ=120 kg/m³, λ=0,042 Vt/m°S, NG — 150 mm, izolyatorni yopishtirish uchun yelim (Technonikol 110) — 10 mm",
                    NameRu = "Утеплитель минераловатные плиты (на основе базальтового волокна) ρ=120 кг/м³, λ=0,042 Вт/м°С, НГ — 150 мм, клей для приклеивания утеплителя (Технониколь 110) — 10 мм", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000037"),
                    Name = "Shtukaturka-yelim qatlami (Technonikol 210), ishqorga chidamli fasad shisha to'r M3600 bilan armirlangan — 4 mm",
                    NameRu = "Штукатурно-клеевой слой (Технониколь 210), армированный фасадной стеклотканевой щелочестойкой сеткой М3600 - 4 мм", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-000000000038"),
                    Name = "Bazalt tolasi asosidagi mineral momiq plitalardan issiqlik izolyatori ρ=120 kg/m³, λ=0,042 Vt/m°S, NG — 100 mm, izolyatorni yopishtirish uchun yelim (Technonikol 110) — 10 mm",
                    NameRu = "Утеплитель минераловатные плиты (на основе базальтового волокна) ρ=120 кг/м³, λ=0,042 Вт/м°С, НГ — 100 мм, клей для приклеивания утеплителя (Технониколь 110) — 10 мм", IsDeleted = false },

            // ── Shift (Потолки) ───────────────────────────────────────────────────
            new() { Id = new("00000000-0000-0000-0012-000000000039"),
                    Name = "Chuqur singdiruvchi grunt (akril suv-emulsion shimdirma) — 1 qavat",
                    NameRu = "Грунтовка глубокого проникновения (пропитка акриловая водоэмульсионная) - 1 слой", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-00000000003A"),
                    Name = "Tortma shift, polivinilxlorid plyonka ρ=230 g/m³, qalinligi 0,2 mm, oq rang, G1, KM2 — 20 mm",
                    NameRu = "Натяжной потолок, пленка поливинилхлорид ρ=230 г/м³, толщ. 0,2 мм, цвет белый, Г1, КМ2 - 20 мм", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-00000000003B"),
                    Name = "Teshiksiz alyuminiy reykali shift, reyka kengligi 130-180 mm, ALBES (qiyqimsiz), polimer-kukun qoplama, oq rang yoki o'xshashi",
                    NameRu = "Алюминиевый реечный потолок без перфорации, ширина рейки 130-180 мм, АЛБЕС (без вставки), полимерно-порошковое покрытие, цвет белый или аналог", IsDeleted = false },
            new() { Id = new("00000000-0000-0000-0012-00000000003C"),
                    Name = "\"Grilyato\" osma shift 75x75x40, metall karkasda, KM0 yong'in xavfsizligi sinfi",
                    NameRu = "Подвесной потолок \"Грилято\" 75х75х40 на мет. каркасе, класс пожарной опасности КМ0", IsDeleted = false },
        };

        // PDF "Смета контракта" bo'yicha har bir ish turiga o'lchov birligi (ЕИ) va
        // birlik narxi (Цена за ЕИ работы, RUB). "вкл"/0.00 → narx boshqa pozitsiyaga kiritilgan (0).
        var unitPrice = new Dictionary<Guid, (Guid UnitId, decimal Price)>
        {
            // ── Pollar (Полы) ──────────────────────────────────────────────────
            [new("00000000-0000-0000-0012-000000000001")] = (UnitM2Id,   25m),    // Разделительный слой плёнка
            [new("00000000-0000-0000-0012-000000000002")] = (UnitM2Id,   420m),   // Стяжка М150 - 84мм
            [new("00000000-0000-0000-0012-000000000003")] = (UnitM2Id,   420m),   // Стяжка М150 - 85мм
            [new("00000000-0000-0000-0012-000000000004")] = (UnitM2Id,   420m),   // Стяжка М150 - 86мм
            [new("00000000-0000-0000-0012-000000000005")] = (UnitM2Id,   420m),   // Стяжка М150 - 88мм
            [new("00000000-0000-0000-0012-000000000006")] = (UnitM2Id,   420m),   // Стяжка М150 - 18мм
            [new("00000000-0000-0000-0012-000000000007")] = (UnitM2Id,   0m),     // Грунтовка глубокого проникновения (вкл)
            [new("00000000-0000-0000-0012-000000000008")] = (UnitM2Id,   0m),     // Плиточный клей, затирка
            [new("00000000-0000-0000-0012-000000000009")] = (UnitM2Id,   650m),   // Керамогранит 600х600х10
            [new("00000000-0000-0000-0012-00000000000A")] = (UnitM2Id,   650m),   // Керамогранит 300х300х10
            [new("00000000-0000-0000-0012-00000000000B")] = (UnitM2Id,   650m),   // Керамогранит морозостойкий 300х300х10
            [new("00000000-0000-0000-0012-00000000000C")] = (UnitM2Id,   100m),   // ТЕПОФОЛ ЭПП
            [new("00000000-0000-0000-0012-00000000000D")] = (UnitM2Id,   0m),     // Подложка под ламинат - 3мм
            [new("00000000-0000-0000-0012-00000000000E")] = (UnitM2Id,   290m),   // Полы из ламината класс 32 - 8мм
            [new("00000000-0000-0000-0012-00000000000F")] = (UnitM2Id,   150m),   // Экструдированный пенополистирол - 200мм
            [new("00000000-0000-0000-0012-000000000010")] = (UnitM2Id,   0m),     // Грунтовка Бетонконтакт (вкл)

            // ── Plintuslar / profillar ───────────────────────────────────────────
            [new("00000000-0000-0000-0012-000000000011")] = (UnitMpId,   100m),   // Плинтус ПВХ (Гостиная, спальня)
            [new("00000000-0000-0000-0012-000000000012")] = (UnitMpId,   100m),   // Плинтус ПВХ (Кухня, Холл)
            [new("00000000-0000-0000-0012-000000000013")] = (UnitMpId,   170m),   // Плинтус керамогранит 400х100х7
            [new("00000000-0000-0000-0012-000000000014")] = (UnitMpId,   170m),   // Плинтус керамогранит 600х95х10
            [new("00000000-0000-0000-0012-000000000015")] = (UnitMpId,   170m),   // Плинтус керамогранит 300х57х7
            [new("00000000-0000-0000-0012-000000000016")] = (UnitMpId,   50m),    // Т-образный профиль 20х1800
            [new("00000000-0000-0000-0012-000000000017")] = (UnitMpId,   150m),   // Калошница 100х50

            // ── Devorlar (Стены) ─────────────────────────────────────────────────
            [new("00000000-0000-0000-0012-000000000018")] = (UnitM2Id,   350m),   // Штукатурка гипс. улучш. - 17мм
            [new("00000000-0000-0000-0012-000000000019")] = (UnitM2Id,   350m),   // Штукатурка гипс. улучш. - 10мм
            [new("00000000-0000-0000-0012-00000000001A")] = (UnitM2Id,   180m),   // Шпатлевка гипс. - 2мм
            [new("00000000-0000-0000-0012-00000000001B")] = (UnitM2Id,   0m),     // Клей для флизелиновых обоев
            [new("00000000-0000-0000-0012-00000000001C")] = (UnitM2Id,   150m),   // Оклейка обоев - 2мм
            [new("00000000-0000-0000-0012-00000000001D")] = (UnitM2Id,   120m),   // Окраска воднодисперс. - 1 слой
            [new("00000000-0000-0000-0012-00000000001E")] = (UnitM2Id,   350m),   // Сплошное выравнивание ЦП - 14мм
            [new("00000000-0000-0000-0012-00000000001F")] = (UnitM2Id,   230m),   // Сплошное выравнивание ЦП - 17мм (по мон.)
            [new("00000000-0000-0000-0012-000000000020")] = (UnitM2Id,   400m),   // Сплошное выравнивание ЦП - 8мм
            [new("00000000-0000-0000-0012-000000000021")] = (UnitM2Id,   400m),   // Сплошное выравнивание ЦП (без указ.)
            [new("00000000-0000-0000-0012-000000000022")] = (UnitM2Id,   700m),   // Фальш стена из ГКЛВ
            [new("00000000-0000-0000-0012-000000000023")] = (UnitM2Id,   230m),   // Шпатлевка ЦП по ГКЛ - 2мм
            [new("00000000-0000-0000-0012-000000000024")] = (UnitM2Id,   100m),   // Гидроизоляция - 2 слоя
            [new("00000000-0000-0000-0012-000000000025")] = (UnitM2Id,   750m),   // Керамогранит 200х300х7 (белый)
            [new("00000000-0000-0000-0012-000000000026")] = (UnitM2Id,   350m),   // Штукатурка ЦП - 17мм (по кладке)
            [new("00000000-0000-0000-0012-000000000027")] = (UnitM2Id,   400m),   // Штукатурка ЦП - 14мм
            [new("00000000-0000-0000-0012-000000000028")] = (UnitM2Id,   230m),   // Шпатлевка ЦП - 2мм
            [new("00000000-0000-0000-0012-000000000029")] = (UnitM2Id,   120m),   // Окраска фасадная Г1 - 1 слой
            [new("00000000-0000-0000-0012-00000000002A")] = (UnitM2Id,   120m),   // Окраска фактурная Г1 - 1 слой
            [new("00000000-0000-0000-0012-00000000002B")] = (UnitM2Id,   120m),   // Окраска простая водоэмульс. Г1 - 1 слой

            // ── Otkoslar, eshiklar ────────────────────────────────────────────────
            [new("00000000-0000-0000-0012-00000000002C")] = (UnitMpId,   0m),     // Грунтовка Бетонконтакт откосов (вкл)
            [new("00000000-0000-0000-0012-00000000002D")] = (UnitMpId,   250m),   // Штукатурка откосов ЦП по сетке
            [new("00000000-0000-0000-0012-00000000002E")] = (UnitMpId,   70m),    // Окраска откосов в 2 слоя
            [new("00000000-0000-0000-0012-00000000002F")] = (UnitMpId,   100m),   // Обрамление откосов ПВХ уголками
            [new("00000000-0000-0000-0012-000000000030")] = (UnitM2Id,   167.33m),// Сетка (перемычки, стыки стен)
            [new("00000000-0000-0000-0012-000000000031")] = (UnitDonaId, 2500m),  // Установка металлических дверей
            [new("00000000-0000-0000-0012-000000000032")] = (UnitDonaId, 2500m),  // Установка межкомнатных дверей

            // ── Fasad ─────────────────────────────────────────────────────────────
            [new("00000000-0000-0000-0012-000000000033")] = (UnitM2Id,   0m),     // Грунтовка фасадная универсальная (вкл)
            [new("00000000-0000-0000-0012-000000000034")] = (UnitM2Id,   350m),   // Утеплитель ТЕХНОФАС 150мм + клей
            [new("00000000-0000-0000-0012-000000000035")] = (UnitM2Id,   350m),   // Штукатурно-клеевой слой 210 - 8мм
            [new("00000000-0000-0000-0012-000000000036")] = (UnitM2Id,   350m),   // Утеплитель базальт 150мм + клей
            [new("00000000-0000-0000-0012-000000000037")] = (UnitM2Id,   350m),   // Штукатурно-клеевой слой 210 - 4мм
            [new("00000000-0000-0000-0012-000000000038")] = (UnitM2Id,   350m),   // Утеплитель базальт 100мм + клей

            // ── Shift (Потолки) ───────────────────────────────────────────────────
            [new("00000000-0000-0000-0012-000000000039")] = (UnitM2Id,   0m),     // Грунтовка (пропитка акрил.) (вкл)
            [new("00000000-0000-0000-0012-00000000003A")] = (UnitM2Id,   498.02m),// Натяжной потолок ПВХ
            [new("00000000-0000-0000-0012-00000000003B")] = (UnitM2Id,   500m),   // Реечный потолок АЛБЕС
            [new("00000000-0000-0000-0012-00000000003C")] = (UnitM2Id,   750m),   // Подвесной потолок "Грилято"
        };

        foreach (var wt in workTypes)
            if (unitPrice.TryGetValue(wt.Id, out var up))
                wt.MeasurementUnitId = up.UnitId;

        // Yangi ish turlarini qo'shish
        var existingIds = await db.WorkTypes.Select(w => w.Id).ToListAsync();
        var newWorkTypes = workTypes.Where(w => !existingIds.Contains(w.Id)).ToList();
        if (newWorkTypes.Count > 0)
        {
            await db.WorkTypes.AddRangeAsync(newWorkTypes);
            await db.SaveChangesAsync();
            logger?.LogInformation("[Seed] {N} ta yangi ish turi", newWorkTypes.Count);
        }

        // Avval seed qilingan ish turlariga o'lchov birligi/narxni to'ldirish (backfill)
        var toBackfill = await db.WorkTypes.AsTracking()
            .Where(w => w.MeasurementUnitId == null)
            .ToListAsync();
        var backfilled = 0;
        foreach (var wt in toBackfill)
            if (unitPrice.TryGetValue(wt.Id, out var up))
            {
                wt.MeasurementUnitId = up.UnitId;
                backfilled++;
            }
        if (backfilled > 0)
        {
            await db.SaveChangesAsync();
            logger?.LogInformation("[Seed] {N} ta ish turiga o'lchov birligi/narx qo'shildi", backfilled);
        }
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
            Item(T1P1Item1Id, T1P1Sec1Id, WorkTypeFloorScreedId, UnitM2Id,
                 450m, 85_000m, 55_000m, 1),
            Item(T1P1Item2Id, T1P1Sec1Id, WorkTypeTileLayingId, UnitM2Id,
                 450m, 120_000m, 80_000m, 2),
            Item(T1P1Item3Id, T1P1Sec2Id, WorkTypeWallPlasterId, UnitM2Id,
                 1_200m, 48_000m, 30_000m, 1),
            Item(T1P1Item4Id, T1P1Sec2Id, WorkTypePaintingId, UnitM2Id,
                 1_200m, 22_000m, 14_000m, 2),
            Item(T1P1Item5Id, T1P1Sec2Id, WorkTypeDrywallId, UnitM2Id,
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
            Item(T1P2Item1Id, T1P2Sec1Id, WorkTypeWindowsId, UnitDonaId,
                 24m, 850_000m, 600_000m, 1),
            Item(T1P2Item2Id, T1P2Sec1Id, WorkTypeDoorsId, UnitDonaId,
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
    // TENANT 1 — Spravochniklar: Ombor, Kassa, Postavshik, Ishlab chiqaruvchi
    // Справочники: Склад, Касса, Поставщик, Производитель
    // ══════════════════════════════════════════════════════════════════════════

    private static async Task SeedTenant1ReferenceDataAsync(UcmsDbContext db, ILogger? logger)
    {
        await SeedTenant1StocksAsync(db, logger);
        await SeedTenant1CashAccountsAsync(db, logger);
        await SeedSuppliersAsync(db, logger);
        await SeedManufacturersAsync(db, logger);
        await SeedProductsAsync(db, logger);
    }

    /// <summary>
    /// Ombor — material va jihozlar saqlanadigan joy. / Склад — место хранения материалов и оборудования.
    /// </summary>
    private static async Task SeedTenant1StocksAsync(UcmsDbContext db, ILogger? logger)
    {
        if (await db.Stocks.AnyAsync(s => s.Id == T1MainStockId))
            return;

        await db.Stocks.AddAsync(new Stock
        {
            Id               = T1MainStockId,
            OrganizationId   = T1OrgId,
            Code             = "OMB-001",
            Name             = "Asosiy ombor",
            NameRu           = "Основной склад",
            NameEn           = "Main warehouse",
            StorageCondition = StorageCondition.Dry,
            StockType        = StockType.Premises,
            StockCategory    = StockCategory.Central,
            IsDeleted        = false,
        });
        await db.SaveChangesAsync();
        logger?.LogInformation("[Seed] TENANT-1: Asosiy ombor yaratildi");
    }

    /// <summary>
    /// Kassa hisoblari — naqd pul kassasi va bank hisob raqami.
    /// Кассовые счета — касса наличных и банковский расчётный счёт.
    /// </summary>
    private static async Task SeedTenant1CashAccountsAsync(UcmsDbContext db, ILogger? logger)
    {
        if (await db.CashAccounts.AnyAsync(c => c.Id == T1CashAccountCashId))
            return;

        var now = Now();

        await db.CashAccounts.AddRangeAsync(
            // Naqd pul kassasi / Касса наличных денежных средств
            new CashAccount
            {
                Id             = T1CashAccountCashId,
                OrganizationId = T1OrgId,
                Name           = "Naqd pul kassasi",
                Type           = CashAccountType.Cash,
                Notes          = "Asosiy naqd pul kassasi",
                IsActive       = true,
                IsDeleted      = false,
                CreatedAt = now, UpdatedAt = now, CreatedBy = T1AdminId, UpdatedBy = T1AdminId,
            },
            // Hisob raqam (bank) / Расчётный счёт (банк)
            new CashAccount
            {
                Id             = T1CashAccountBankId,
                OrganizationId = T1OrgId,
                Name           = "Hisob raqam",
                Type           = CashAccountType.Bank,
                Notes          = "Asosiy bank hisob raqami",
                IsActive       = true,
                IsDeleted      = false,
                CreatedAt = now, UpdatedAt = now, CreatedBy = T1AdminId, UpdatedBy = T1AdminId,
            }
        );
        await db.SaveChangesAsync();
        logger?.LogInformation("[Seed] TENANT-1: 2 ta kassa hisobi yaratildi (naqd + hisob raqam)");
    }

    /// <summary>
    /// Postavshik — material/tovar yetkazib beruvchi tashkilot. / Поставщик материалов и товаров.
    /// </summary>
    private static async Task SeedSuppliersAsync(UcmsDbContext db, ILogger? logger)
    {
        if (await db.Suppliers.AnyAsync(s => s.Id == Supplier1Id))
            return;

        await db.Suppliers.AddAsync(new Supplier
        {
            Id        = Supplier1Id,
            Code      = "POST-001",
            Name      = "Qurilish Materiallari Savdo MChJ",
            NameRu    = "ООО \"Торговля строительными материалами\"",
            NameEn    = "Construction Materials Trade LLC",
            IsDeleted = false,
        });
        await db.SaveChangesAsync();
        logger?.LogInformation("[Seed] Postavshik yaratildi");
    }

    /// <summary>
    /// Ishlab chiqaruvchi — mahsulot/material ishlab chiqaruvchi tashkilot. / Производитель продукции/материалов.
    /// </summary>
    private static async Task SeedManufacturersAsync(UcmsDbContext db, ILogger? logger)
    {
        if (await db.Manufacturers.AnyAsync(m => m.Id == Manufacturer1Id))
            return;

        await db.Manufacturers.AddAsync(new Manufacturer
        {
            Id        = Manufacturer1Id,
            Code      = "ISHLAB-001",
            Name      = "Qurilish Materiallari Ishlab Chiqarish Zavodi",
            NameRu    = "Завод по производству строительных материалов",
            NameEn    = "Construction Materials Manufacturing Plant",
            IsDeleted = false,
        });
        await db.SaveChangesAsync();
        logger?.LogInformation("[Seed] Ishlab chiqaruvchi yaratildi");
    }

    /// <summary>
    /// Qurilishda eng ko'p ishlatiladigan 5 ta mahsulot (har biriga 1 ta SKU bilan).
    /// 5 самых часто используемых в строительстве продуктов (каждый с одним SKU).
    /// </summary>
    private static async Task SeedProductsAsync(UcmsDbContext db, ILogger? logger)
    {
        if (await db.Products.AnyAsync(p => p.Id == ProductCementId))
            return;

        await db.Products.AddRangeAsync(
            new Product
            {
                Id     = ProductCementId,
                Code   = "PROD-001",
                Name   = "Sement (M-400)",
                NameRu = "Цемент (М-400)",
                NameEn = "Cement (M-400)",
                Type   = ProductType.Cement,
                IsDeleted = false,
            },
            new Product
            {
                Id     = ProductBrickId,
                Code   = "PROD-002",
                Name   = "Silikat g'isht",
                NameRu = "Силикатный кирпич",
                NameEn = "Silicate brick",
                Type   = ProductType.Brick,
                IsDeleted = false,
            },
            new Product
            {
                Id     = ProductRebarId,
                Code   = "PROD-003",
                Name   = "Armatura (Ø12mm)",
                NameRu = "Арматура (Ø12мм)",
                NameEn = "Rebar (Ø12mm)",
                Type   = ProductType.Rebar,
                IsDeleted = false,
            },
            new Product
            {
                Id     = ProductTileId,
                Code   = "PROD-004",
                Name   = "Keramogranit plitka (600x600)",
                NameRu = "Керамогранитная плитка (600x600)",
                NameEn = "Porcelain tile (600x600)",
                Type   = ProductType.Tile,
                IsDeleted = false,
            },
            new Product
            {
                Id     = ProductPaintId,
                Code   = "PROD-005",
                Name   = "Fasad bo'yog'i",
                NameRu = "Фасадная краска",
                NameEn = "Facade paint",
                Type   = ProductType.Paint,
                IsDeleted = false,
            }
        );

        await db.Skus.AddRangeAsync(
            new Sku
            {
                Id                  = SkuCementId,
                ProductId           = ProductCementId,
                ManufacturerId      = Manufacturer1Id,
                SupplierId          = Supplier1Id,
                MeasurementUnitId   = UnitTonId,
                SerialNumber        = "SKU-CEMENT-001",
                Amount              = 100,
                Price               = 850_000m,
                ExpirationDate      = Now().AddMonths(6),
                Status              = SkuStatus.Default,
                IsDeleted           = false,
            },
            new Sku
            {
                Id                  = SkuBrickId,
                ProductId           = ProductBrickId,
                ManufacturerId      = Manufacturer1Id,
                SupplierId          = Supplier1Id,
                MeasurementUnitId   = UnitDonaId,
                SerialNumber        = "SKU-BRICK-001",
                Amount              = 5000,
                Price               = 1_200m,
                ExpirationDate      = Now().AddYears(5),
                Status              = SkuStatus.Default,
                IsDeleted           = false,
            },
            new Sku
            {
                Id                  = SkuRebarId,
                ProductId           = ProductRebarId,
                ManufacturerId      = Manufacturer1Id,
                SupplierId          = Supplier1Id,
                MeasurementUnitId   = UnitTonId,
                SerialNumber        = "SKU-REBAR-001",
                Amount              = 20,
                Price               = 9_500_000m,
                ExpirationDate      = Now().AddYears(10),
                Status              = SkuStatus.Default,
                IsDeleted           = false,
            },
            new Sku
            {
                Id                  = SkuTileId,
                ProductId           = ProductTileId,
                ManufacturerId      = Manufacturer1Id,
                SupplierId          = Supplier1Id,
                MeasurementUnitId   = UnitM2Id,
                SerialNumber        = "SKU-TILE-001",
                Amount              = 800,
                Price               = 95_000m,
                ExpirationDate      = Now().AddYears(15),
                Status              = SkuStatus.Default,
                IsDeleted           = false,
            },
            new Sku
            {
                Id                  = SkuPaintId,
                ProductId           = ProductPaintId,
                ManufacturerId      = Manufacturer1Id,
                SupplierId          = Supplier1Id,
                MeasurementUnitId   = UnitKgId,
                SerialNumber        = "SKU-PAINT-001",
                Amount              = 300,
                Price               = 75_000m,
                ExpirationDate      = Now().AddYears(2),
                Status              = SkuStatus.Default,
                IsDeleted           = false,
            }
        );

        await db.SaveChangesAsync();
        logger?.LogInformation("[Seed] 5 ta mahsulot va SKU yaratildi (sement, g'isht, armatura, plitka, bo'yoq)");
    }

    // ══════════════════════════════════════════════════════════════════════════
    // IXTIYOR — shaxsiy qurilish pudratchisi tashkiloti (Daminov Ixtiyor Ilhomjonovich)
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

        // direktor — Daminov Ixtiyor IlhomIlhomjonovich, o'z tashkilotida to'liq huquq (Admin rol)
        await CreateUserAsync(um, logger, new User
        {
            Id                 = IhtiyorDirectorUserId,
            UserName           = "ixtiyor.direktor",
            NormalizedUserName = "IXTIYOR.DIREKTOR",
            Email              = "ixtiyor.pudrat@gmail.com",
            NormalizedEmail    = "IXTIYOR.PUDRAT@GMAIL.COM",
            EmailConfirmed     = true,
            FullName           = "Daminov Ixtiyor Ilhomjonovich",
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
                Name           = "Daminov Ixtiyor Ilhomjonovich",
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
            CreatedAt      = now, 
            UpdatedAt = now,
            CreatedBy      = IhtiyorDirectorUserId, 
            UpdatedBy = IhtiyorDirectorUserId,
        };

        var est = new Estimate
        {
            Id          = IhtiyorEstId,
            ProjectId   = IhtiyorProjectId,
            Name        = "Smeta kontrakti (2,3-sektsiya otdelka)",
            Description = "Zakazchik (OOO IKS) bilan tuzilgan shartnoma narxining tafsilnomasi",
            Order       = 1,
            CreatedAt   = now, 
            UpdatedAt = now,
            CreatedBy   = IhtiyorDirectorUserId, 
            UpdatedBy = IhtiyorDirectorUserId,
        };

        var sec1 = Sec(IhtiyorSec1Id, IhtiyorEstId, "Pol ishlari", 1);
        var sec2 = Sec(IhtiyorSec2Id, IhtiyorEstId, "Devor ishlari", 2);

        var items = new[]
        {
            // ── Pol ishlari ──
            Item(IhtiyorItem1Id, IhtiyorSec1Id,
                 WorkTypeVaporBarrierId, UnitM2Id,
                 2360.53m, 56.57m, 25m, 1),
            Item(IhtiyorItem2Id, IhtiyorSec1Id,
                 WorkTypeSemiDryScreedId, UnitM2Id,
                 2360.53m, 932.29m, 420m, 2),
            Item(IhtiyorItem3Id, IhtiyorSec1Id,
                 WorkTypePorcelainTileId, UnitM2Id,
                 2360.53m, 1195.24m, 650m, 3),
            // ── Devor ishlari ──
            Item(IhtiyorItem4Id, IhtiyorSec2Id,
                 WorkTypeGypsumPlasterId, UnitM2Id,
                 7980.40m, 733.08m, 350m, 1),
            Item(IhtiyorItem5Id, IhtiyorSec2Id,
                 WorkTypeGypsumPuttyId, UnitM2Id,
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

    /// <summary>
    /// IXTIYOR tashkiloti uchun spravochniklar: ombor va kassa hisoblari.
    /// Справочники для организации ИХТИЁР: склад и кассовые счета.
    /// </summary>
    private static async Task SeedIhtiyorReferenceDataAsync(UcmsDbContext db, ILogger? logger)
    {
        await SeedIhtiyorStockAsync(db, logger);
        await SeedIhtiyorCashAccountsAsync(db, logger);
    }

    /// <summary>
    /// Ombor — material va jihozlar saqlanadigan joy. / Склад — место хранения материалов и оборудования.
    /// </summary>
    private static async Task SeedIhtiyorStockAsync(UcmsDbContext db, ILogger? logger)
    {
        if (await db.Stocks.AnyAsync(s => s.Id == IhtiyorStockId))
            return;

        await db.Stocks.AddAsync(new Stock
        {
            Id               = IhtiyorStockId,
            OrganizationId   = IhtiyorOrgId,
            Code             = "OMB-001",
            Name             = "Asosiy ombor",
            NameRu           = "Основной склад",
            NameEn           = "Main warehouse",
            StorageCondition = StorageCondition.Dry,
            StockType        = StockType.Premises,
            StockCategory    = StockCategory.Central,
            IsDeleted        = false,
        });
        await db.SaveChangesAsync();
        logger?.LogInformation("[Seed] IXTIYOR: Asosiy ombor yaratildi");
    }

    /// <summary>
    /// Kassa hisoblari — naqd pul kassasi va bank hisob raqami.
    /// Кассовые счета — касса наличных и банковский расчётный счёт.
    /// </summary>
    private static async Task SeedIhtiyorCashAccountsAsync(UcmsDbContext db, ILogger? logger)
    {
        if (await db.CashAccounts.AnyAsync(c => c.Id == IhtiyorCashAccountCashId))
            return;

        var now = Now();

        await db.CashAccounts.AddRangeAsync(
            // Naqd pul kassasi / Касса наличных денежных средств
            new CashAccount
            {
                Id             = IhtiyorCashAccountCashId,
                OrganizationId = IhtiyorOrgId,
                Name           = "Naqd pul kassasi",
                Type           = CashAccountType.Cash,
                Notes          = "Asosiy naqd pul kassasi",
                IsActive       = true,
                IsDeleted      = false,
                CreatedAt = now, UpdatedAt = now, CreatedBy = IhtiyorDirectorUserId, UpdatedBy = IhtiyorDirectorUserId,
            },
            // Hisob raqam (bank) / Расчётный счёт (банк)
            new CashAccount
            {
                Id             = IhtiyorCashAccountBankId,
                OrganizationId = IhtiyorOrgId,
                Name           = "Hisob raqam",
                Type           = CashAccountType.Bank,
                Notes          = "Asosiy bank hisob raqami",
                IsActive       = true,
                IsDeleted      = false,
                CreatedAt = now, UpdatedAt = now, CreatedBy = IhtiyorDirectorUserId, UpdatedBy = IhtiyorDirectorUserId,
            }
        );
        await db.SaveChangesAsync();
        logger?.LogInformation("[Seed] IXTIYOR: 2 ta kassa hisobi yaratildi (naqd + hisob raqam)");
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
        Guid id, Guid sectionId, Guid workTypeId, Guid measurementUnitId,
        decimal volume, decimal clientPrice, decimal brigadePrice, int order)
    {
        return new()
        {
            Id                = id,
            SectionId         = sectionId,
            WorkTypeId        = workTypeId,
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
