using GovcoreBse.Store.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using GovcoreBse.Shared.Tools;
using Cortex.Mediator.Commands;
using Cortex.Mediator.DependencyInjection;
using Cortex.Mediator.Queries;

namespace GovcoreBse.Store.Setup
{
    public static class InjectService
    {
        
        public static IServiceCollection AddCommandMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg => { },Assembly.GetExecutingAssembly());
            services.AddCortexMediator(new[] { typeof(InjectService) },cfg =>
            {
                cfg.AddOpenQueryPipelineBehavior(typeof(LoggingPipelineBehavior<,>));

            });


            return services;
        }

        public static IServiceCollection AddStore<T>(this IServiceCollection services) where T : class, IN.IDBSetting
        {
            services.AddScoped<IBlazeLogDbContext,BlazeLogDbContext>();



            // Add MSSQL DB Context
            services.AddDbContext<BlazeLogDbContext>((provider,options) =>
            {
                var option = provider.GetRequiredService<IOptions<T>>();
                var encryp = provider.GetRequiredService<StringEncrypService>();
                IN.IDBSetting value = option.Value;
                var builder = new SqlConnectionStringBuilder();
                builder.DataSource = value.DBsource;
                builder.InitialCatalog = value.DBcatalog;                
                builder.PersistSecurityInfo = true;
                builder.MultipleActiveResultSets = true;
                builder.TrustServerCertificate = true;
                builder.UserID = value.DBuser;
                builder.Password = encryp.DecryptString(value.DBpwd);
                var connc = builder.ToString(); 
                options.UseSqlServer(connc, providerOptions => providerOptions.EnableRetryOnFailure()).EnableDetailedErrors();

            });

            return services;
        }
    }
}
