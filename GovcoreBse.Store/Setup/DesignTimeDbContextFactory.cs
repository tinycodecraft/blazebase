using GovcoreBse.Store.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using GovcoreBse.Shared;

namespace GovcoreBse.Store;

public class DesignTimeDbContextFactory:IDesignTimeDbContextFactory<BlazeLogDbContext>
{
    
    public DesignTimeDbContextFactory()
    {

    }

    public BlazeLogDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(@Directory.GetCurrentDirectory() + $"/../{CN.Setting.AppName}/appsettings.json")
            .Build();
        var builder = new DbContextOptionsBuilder<BlazeLogDbContext>();
        var connectionString = configuration.GetConnectionString(nameof(BlazeLogDbContext).Replace(nameof(DbContext),""));
        builder.UseSqlServer(connectionString, opt => opt.MigrationsAssembly($"{CN.Setting.AppName}.Store"));
        return new BlazeLogDbContext(builder.Options);

    }
}
