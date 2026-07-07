namespace Ucms.Application.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Ucms.Domain.Entities;
using Ucms.Domain.Entities.Identity;

public interface IUcmsDbContext
{
    // Tashkilot
    public DbSet<Organization> Organizations { get; set; }

    // Loyiha va smeta
    public DbSet<Project> Projects { get; set; }
    public DbSet<Estimate> Estimates { get; set; }
    public DbSet<EstimateSection> EstimateSections { get; set; }
    public DbSet<EstimateItem> EstimateItems { get; set; }

    // Brigadalar
    public DbSet<Brigade> Brigades { get; set; }

    // Bajarilgan ishlar
    public DbSet<WorkLog> WorkLogs { get; set; }

    // Zakazchik akti va to'lovlar
    public DbSet<ClientAct> ClientActs { get; set; }
    public DbSet<ClientActItem> ClientActItems { get; set; }
    public DbSet<ClientPayment> ClientPayments { get; set; }

    // Brigada to'lovlari
    public DbSet<BrigadePayment> BrigadePayments { get; set; }

    // Loyiha xarajatlari
    public DbSet<ProjectExpense> ProjectExpenses { get; set; }

    // Xodimlar
    public DbSet<Employee> Employees { get; set; }

    // Maoshlar
    public DbSet<Salary> Salaries { get; set; }

    // Buyurtmachilar
    public DbSet<Customer> Customers { get; set; }

    // Finance (kassa va pul harakati)
    public DbSet<CashAccount> CashAccounts { get; set; }
    public DbSet<CashTransaction> CashTransactions { get; set; }
    public DbSet<AccountTransfer> AccountTransfers { get; set; }

    // O'lchov birliklari (spravochnik)
    public DbSet<MeasurementUnit> MeasurementUnits { get; set; }
    public DbSet<OrganizationMeasurementUnit> OrganizationMeasurementUnits { get; set; }

    // Ish turlari (spravochnik)
    public DbSet<WorkType> WorkTypes { get; set; }

    // Mahsulotlar va ishlab chiqaruvchilar
    public DbSet<Product>      Products      { get; set; }
    public DbSet<Manufacturer> Manufacturers { get; set; }
    public DbSet<Supplier>     Suppliers     { get; set; }
    public DbSet<Sku>          Skus          { get; set; }

    // Ombor
    public DbSet<Stock>                Stocks                { get; set; }
    public DbSet<StockSku>             StockSkus             { get; set; }
    public DbSet<StockDemand>          StockDemands          { get; set; }
    public DbSet<StockDemandItem>      StockDemandItems      { get; set; }
    public DbSet<StockBalanceRegister> StockBalanceRegisters { get; set; }
    public DbSet<OrganizationSku>      OrganizationSkus      { get; set; }

    // Kirim va chiqim
    public DbSet<Income>        Incomes        { get; set; }
    public DbSet<IncomeItem>    IncomeItems    { get; set; }
    public DbSet<IncomeOutcome> IncomeOutcomes { get; set; }
    public DbSet<Outcome>       Outcomes       { get; set; }
    public DbSet<OutcomeItem>   OutcomeItems   { get; set; }

    // Identity
    public DbSet<User>         Users         { get; set; }
    public DbSet<Role>         Roles         { get; set; }
    public DbSet<UserRole>     UserRoles     { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public int SaveChanges();
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    public EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
    public IExecutionStrategy CreateExecutionStrategy();
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    public DatabaseFacade Database { get; }

    /// <summary>
    /// Change tracker'dagi barcha tracked entity'larni tozalaydi.
    /// CreateExecutionStrategy().ExecuteAsync() ichida retry bo'lganda stale state ni oldini olish uchun
    /// har bir urinish boshida chaqiriladi.
    /// </summary>
    public void ClearChangeTracker();
}
