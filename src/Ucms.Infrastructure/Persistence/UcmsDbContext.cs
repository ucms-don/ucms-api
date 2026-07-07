namespace Ucms.Infrastructure.Persistence;

using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Common;
using Ucms.Domain.Entities;
using Ucms.Domain.Entities.Identity;

public class UcmsDbContext(
    DbContextOptions<UcmsDbContext> options,
    ICurrentContext context)
    : IdentityDbContext<
        User, Role, Guid,
        UserClaim, UserRole, UserLogin,
        RoleClaim, UserToken>(options),
      IUcmsDbContext
{
    // ── Tashkilot ──────────────────────────────────────────────────────────
    public DbSet<Organization> Organizations { get; set; }

    // ── Loyiha va smeta ────────────────────────────────────────────────────
    public DbSet<Project> Projects { get; set; }
    public DbSet<Estimate> Estimates { get; set; }
    public DbSet<EstimateSection> EstimateSections { get; set; }
    public DbSet<EstimateItem> EstimateItems { get; set; }

    // ── Brigadalar ─────────────────────────────────────────────────────────
    public DbSet<Brigade> Brigades { get; set; }

    // ── Bajarilgan ishlar ──────────────────────────────────────────────────
    public DbSet<WorkLog> WorkLogs { get; set; }

    // ── Zakazchik akti va to'lovlar ────────────────────────────────────────
    public DbSet<ClientAct> ClientActs { get; set; }
    public DbSet<ClientActItem> ClientActItems { get; set; }
    public DbSet<ClientPayment> ClientPayments { get; set; }

    // ── Brigada to'lovlari ─────────────────────────────────────────────────
    public DbSet<BrigadePayment> BrigadePayments { get; set; }

    // ── Loyiha xarajatlari ─────────────────────────────────────────────────
    public DbSet<ProjectExpense> ProjectExpenses { get; set; }

    // ── Xodimlar ───────────────────────────────────────────────────────────
    public DbSet<Employee> Employees { get; set; }

    // ── Maoshlar ───────────────────────────────────────────────────────────
    public DbSet<Salary> Salaries { get; set; }

    // ── Buyurtmachilar ─────────────────────────────────────────────────────
    public DbSet<Customer> Customers { get; set; }

    // ── Finance (kassa va pul harakati) ───────────────────────────────────
    public DbSet<CashAccount> CashAccounts { get; set; }
    public DbSet<CashTransaction> CashTransactions { get; set; }
    public DbSet<AccountTransfer> AccountTransfers { get; set; }

    // ── Spravochniklar ─────────────────────────────────────────────────────
    public DbSet<MeasurementUnit>             MeasurementUnits             { get; set; }
    public DbSet<OrganizationMeasurementUnit> OrganizationMeasurementUnits { get; set; }
    public DbSet<WorkType>                    WorkTypes                    { get; set; }

    // ── Mahsulotlar ────────────────────────────────────────────────────────
    public DbSet<Product>      Products      { get; set; }
    public DbSet<Manufacturer> Manufacturers { get; set; }
    public DbSet<Supplier>     Suppliers     { get; set; }
    public DbSet<Sku>          Skus          { get; set; }

    // ── Ombor ──────────────────────────────────────────────────────────────
    public DbSet<Stock>                Stocks                { get; set; }
    public DbSet<StockSku>             StockSkus             { get; set; }
    public DbSet<StockDemand>          StockDemands          { get; set; }
    public DbSet<StockDemandItem>      StockDemandItems      { get; set; }
    public DbSet<StockBalanceRegister> StockBalanceRegisters { get; set; }
    public DbSet<OrganizationSku>      OrganizationSkus      { get; set; }

    // ── Kirim va chiqim ────────────────────────────────────────────────────
    public DbSet<Income>        Incomes        { get; set; }
    public DbSet<IncomeItem>    IncomeItems    { get; set; }
    public DbSet<IncomeOutcome> IncomeOutcomes { get; set; }
    public DbSet<Outcome>       Outcomes       { get; set; }
    public DbSet<OutcomeItem>   OutcomeItems   { get; set; }

    // ── Identity (override) ────────────────────────────────────────────────
    public override DbSet<User> Users { get; set; }
    public override DbSet<Role> Roles { get; set; }
    public override DbSet<UserRole> UserRoles { get; set; }
    public override DbSet<RoleClaim> RoleClaims { get; set; }

    // ── Auth ───────────────────────────────────────────────────────────────
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    // ── IUcmsDbContext infra ────────────────────────────────────────────────
    public IExecutionStrategy CreateExecutionStrategy()
    {
        return Database.CreateExecutionStrategy();
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        return await Database.BeginTransactionAsync(ct);
    }

    public void ClearChangeTracker()
    {
        ChangeTracker.Clear();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── Identity jadvallari → "Identity" schemasi ──────────────────────
        builder.Entity<User>().ToTable("Users", "Identity");
        builder.Entity<Role>().ToTable("Roles", "Identity");
        builder.Entity<UserRole>().ToTable("UserRoles", "Identity");
        builder.Entity<UserClaim>().ToTable("UserClaims", "Identity");
        builder.Entity<UserLogin>().ToTable("UserLogins", "Identity");
        builder.Entity<RoleClaim>().ToTable("RoleClaims", "Identity");
        builder.Entity<UserToken>().ToTable("UserTokens", "Identity");
        builder.Entity<RefreshToken>().ToTable("RefreshTokens", "Identity");

        // User ↔ Role navigations
        builder.Entity<User>()
            .HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();

        builder.Entity<Role>()
            .HasMany(r => r.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId)
            .IsRequired();

        // User ↔ Employee (1:1, optional)
        builder.Entity<User>()
            .HasOne<Employee>()
            .WithOne()
            .HasForeignKey<User>(u => u.EmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        // RefreshToken
        builder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId);

        // ── Domain konfiguratsiyalari ───────────────────────────────────────
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // ── Global query filterlar ─────────────────────────────────────────
        ApplyGlobalFilters(builder);

        // ── IDeletable entitylar uchun IsDeleted indexlari ─────────────────
        ApplySoftDeleteIndexes(builder);
    }

    // ── SaveChanges ────────────────────────────────────────────────────────
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        ApplyAuditInfo();
        var result = await base.SaveChangesAsync(ct);
        await PublishDomainEventsAsync(ct);
        return result;
    }

    // ── Global filters ─────────────────────────────────────────────────────
    /// <summary>
    /// Barcha entity turlari uchun soft-delete va organization filtrlari
    /// </summary>
    private void ApplyGlobalFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            var hasOrg = typeof(IHasOrganization).IsAssignableFrom(clrType);
            var isDeletable = typeof(IDeletable).IsAssignableFrom(clrType);

            if (!isDeletable)
                continue;

            if (hasOrg)
            {
                // Organization + soft-delete filtri
                GetType()
                    .GetMethod(nameof(SetOrgAndDeleteFilter), BindingFlags.NonPublic | BindingFlags.Instance)!
                    .MakeGenericMethod(clrType)
                    .Invoke(this, [modelBuilder]);
            }
            else
            {
                // Faqat soft-delete filtri
                typeof(UcmsDbContext)
                    .GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(clrType)
                    .Invoke(null, [modelBuilder]);
            }
        }
    }

    /// <summary>
    /// IDeletable barcha entity turlar uchun IsDeleted ustuniga index qo'shadi.
    /// Partial index emas — EF Core Npgsql bu sintaksisni migrationda to'g'ri chiqaradi.
    /// </summary>
    private static void ApplySoftDeleteIndexes(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IDeletable).IsAssignableFrom(entityType.ClrType))
                continue;

            var tableName = entityType.GetTableName() ?? entityType.ClrType.Name;
            modelBuilder.Entity(entityType.ClrType)
                .HasIndex("IsDeleted")
                .HasDatabaseName($"IX_{tableName}_IsDeleted");
        }
    }

    private void SetOrgAndDeleteFilter<T>(ModelBuilder modelBuilder)
        where T : class, IDeletable, IHasOrganization
    {
        // Owner foydalanuvchilar barcha tashkilotlar ma'lumotlarini ko'radi
        modelBuilder.Entity<T>().HasQueryFilter(e =>
                !e.IsDeleted &&
                (context.IsOwner ||
                 context.OrganizationId == null ||
                 e.OrganizationId == context.OrganizationId));
    }

    private static void SetSoftDeleteFilter<T>(ModelBuilder modelBuilder)
        where T : class, IDeletable
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
    }

    // ── Audit info ─────────────────────────────────────────────────────────
    /// <summary>
    /// Qo'shilgan/o'zgartirilgan entitylarga CreatedAt, UpdatedAt, CreatedBy, UpdatedBy yozadi
    /// </summary>
    private void ApplyAuditInfo()
    {
        var now = DateTimeOffset.UtcNow;
        var userId = context.UserId;
        var orgId = context.OrganizationId;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    if (userId.HasValue)
                    {
                        entry.Entity.CreatedBy = userId.Value;
                        entry.Entity.UpdatedBy = userId.Value;
                    }
                    // IHasOrganization bo'lsa va OrganizationId bo'sh bo'lsa — contextdan to'ldiradi
                    if (orgId.HasValue &&
                        entry.Entity is IHasOrganization hasOrg &&
                        hasOrg.OrganizationId == Guid.Empty)
                    {
                        hasOrg.OrganizationId = orgId.Value;
                    }
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    if (userId.HasValue)
                        entry.Entity.UpdatedBy = userId.Value;
                    break;
            }
        }

        // Identity User audit
        foreach (var entry in ChangeTracker.Entries<User>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    if (userId.HasValue)
                    {
                        entry.Entity.CreatedBy = userId.Value;
                        entry.Entity.UpdatedBy = userId.Value;
                    }
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    if (userId.HasValue)
                        entry.Entity.UpdatedBy = userId.Value;
                    break;
            }
        }
    }

    // ── Domain events ──────────────────────────────────────────────────────
    /// <summary>
    /// Entity lardagi domain eventlarni tozalaydi.
    /// MassTransit consumer lar tayyor bo'lgandan keyin publish qo'shiladi.
    /// </summary>
    private Task PublishDomainEventsAsync(CancellationToken ct)
    {
        var entities = ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        entities.ForEach(e => e.ClearDomainEvents());

        return Task.CompletedTask;
    }
}
