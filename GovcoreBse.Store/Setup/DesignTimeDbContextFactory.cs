using GovcoreBse.Shared;
using GovcoreBse.Shared.Tools;
using GovcoreBse.Store.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace GovcoreBse.Store;

public class DesignTimeDbContextFactory:IDesignTimeDbContextFactory<BlazeLogDbContext>
{
    
    public DesignTimeDbContextFactory()
    {

    }

    public BlazeLogDbContext CreateDbContext(string[] args)
    {
        //i.e. dotnet ef migrations add InitialCreate --project ./GovcoreBse.Store/GovcoreBse.Store.csproj --startup-project ./GovcoreBse/GovcoreBse.csproj -- HomeDevelop
        //
        // 預設為 Development，如果命令列有傳入參數則覆蓋
        string environment = "HomeDevelop";

        if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
        {
            environment = args[0];
        }
        
        IConfigurationRoot configuration = new ConfigurationBuilder()
            // AppContext.BaseDirectory 會指向 dotnet ef 執行時的暫存或啟動目錄  assume the appsettings.json in the project specified by --startup-project
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            //.SetBasePath(Directory.GetCurrentDirectory())
            //.AddJsonFile(@Directory.GetCurrentDirectory() + $"/../{CN.Setting.AppName}/appsettings.json")
            .Build();
        var builder = new DbContextOptionsBuilder<BlazeLogDbContext>();
        DBRCUSetting dbSetting = new DBRCUSetting();
        configuration.GetSection(CN.Setting.DBRCUSetting).Bind(dbSetting);

        var encrypsvc = new StringEncrypService();
        var sqlbuilder = new SqlConnectionStringBuilder();
        sqlbuilder.DataSource = dbSetting.DBsource;
        sqlbuilder.InitialCatalog = dbSetting.DBcatalog;
        sqlbuilder.PersistSecurityInfo = true;
        sqlbuilder.MultipleActiveResultSets = true;
        sqlbuilder.TrustServerCertificate = true;
        sqlbuilder.UserID = dbSetting.DBuser;
        sqlbuilder.Password = encrypsvc.DecryptString(dbSetting.DBpwd);
        var connc = sqlbuilder.ToString();
        
        builder.UseSqlServer(connc, opt => opt.MigrationsAssembly($"{CN.Setting.AppName}.Store"));
        return new BlazeLogDbContext(builder.Options);

    }
}
