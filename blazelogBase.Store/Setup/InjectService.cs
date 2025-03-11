using blazelogBase.Store.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blazelogBase.Store.Setup
{
    public static class InjectService
    {
        

        public static IServiceCollection AddStore(this IServiceCollection services, IConfiguration confg,string conn="BlazeLog")
        {
            services.AddScoped<IBlazeLogDbContext,BlazeLogDbContext>();

            var connc = confg.GetConnectionString(conn);

            // Add MSSQL DB Context
            services.AddDbContext<BlazeLogDbContext>(options =>
            {
                options.UseSqlServer(connc, providerOptions => providerOptions.EnableRetryOnFailure()).EnableDetailedErrors();

            });

            return services;
        }
    }
}
