using blazelogBase.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace blazelogBase.Middlewares;

public static class HelperExtensions
{
    public static void SetLangCookie(this HttpContext ctx,string? lang="en-US",int year=0,int month=0,int day=1)
    {
        var cookieOptions = new CookieOptions
        {
            Expires = DateTimeOffset.Now.AddYears(year).AddMonths(month).AddDays(day),
            IsEssential = true,
        };

        ctx.Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(lang ?? "en-US")),
            cookieOptions
        );
        
    }
}

public static class ExceptionHandlerExtensions
{
    //Simple handler
    public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(builder =>
        {
            builder.Run(async context =>
            {
                var error = context.Features.Get<IExceptionHandlerFeature>();
                var exDetails = new ExceptionDetails((int)HttpStatusCode.InternalServerError, error?.Error.Message ?? "");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = exDetails.StatusCode;
                context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                context.Response.Headers.Add("Application-Error", exDetails.Message);
                context.Response.Headers.Add("Access-Control-Expose-Headers", "Application-Error");

                await context.Response.WriteAsync(exDetails.ToString());
            });
        });

        return app;
    }

    //custom handler with logging
    public static IApplicationBuilder UseApiExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ApiExceptionHandlingMiddleware>();
}
public static class ServiceCollectionExtensions
{




    public static IServiceCollection AddCustomLocalization(this IServiceCollection services, params string[] langs)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources");

        //without country code "-xx" suffix => culture invariant

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.SetDefaultCulture(langs[0])
            .AddSupportedCultures(langs)
                .AddSupportedUICultures(langs);

        });

        return services;
    }
}