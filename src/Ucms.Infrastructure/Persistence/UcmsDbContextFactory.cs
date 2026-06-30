namespace Ucms.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

public class UcmsDbContextFactory : IDesignTimeDbContextFactory<UcmsDbContext>
{
    public UcmsDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Ucms.Api"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var options = new DbContextOptionsBuilder<UcmsDbContext>()
            .UseNpgsql(config.GetConnectionString("DefaultConnection"))
            .Options;

        return new UcmsDbContext(options, null!); // design-time only
    }
}
