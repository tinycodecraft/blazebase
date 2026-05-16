using GovcoreBse.Manner;
using GovcoreBse.Models;
using GovcoreBse.Store.Setup;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;

namespace GovcoreBse.Middlewares;

//let the controller to use this.RazorView<T>(model) instead of View(model)
public static class ControllerExtensions
{
    public static IResult RazorView<T>(this Controller controller) where T : IComponent
    {
        return new RazorComponentResult<T>();
    }

    public static IResult RazorView<T>(this Controller controller, IReadOnlyDictionary<string, object?> parameters) where T : IComponent
    {
        return new RazorComponentResult<T>(parameters);
    }

    public static IResult RazorView<T>(this Controller controller, object parameters) where T : IComponent
    {
        return new RazorComponentResult<T>(parameters);
    }

    public static IResult RazorView(this Controller controller, Type componentType)
    {
        return new RazorComponentResult(componentType);
    }

    public static IResult RazorView(this Controller controller, Type componentType, IReadOnlyDictionary<string, object?> parameters)
    {
        return new RazorComponentResult(componentType, parameters);
    }

    public static IResult RazorView(this Controller controller, Type componentType, object parameters)
    {
        return new RazorComponentResult(componentType, parameters);
    }
}

public static class HelperExtensions
{
    public static void SetLangCookie(this HttpContext ctx,string? lang=DK.LANG_ENG,int year=0,int month=0,int day=1)
    {
        var cookieOptions = new CookieOptions
        {
            Expires = DateTimeOffset.Now.AddYears(year).AddMonths(month).AddDays(day),
            IsEssential = true,
        };

        ctx.Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(lang ?? DK.LANG_ENG)),
            cookieOptions
        );
        
    }
}

public static class MiniApisExtensions
{
    public static IEndpointRouteBuilder UseApisMapping(this IEndpointRouteBuilder app,params CN.AutocompleteGroup[] groups)
    {
        foreach(var g in groups)
        {
            app.MapGroup(CN.Setting.MiniApiPathPrefix + HelperT.ToNameString(g))
                .MapApiFor(g)
                .WithTags(HelperT.ToNameString(g));
        }
        return app;
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

public static class ClaimsIdentityHandlerExtensions
{
    //this resolver must be placed in the first line after builder build statement.
    public static IApplicationBuilder  UseClaimsIdentityResolver(this IApplicationBuilder app)
    {
        app.Use(async (context, next) => {

            var appman = context.RequestServices.GetRequiredService<AppManager>();
            if(appman!=null && appman.UserState!=null)
            {
                var userState = appman.UserState;
                var db = context.RequestServices.GetRequiredService<IBlazeLogDbContext>();
                if (db != null)
                {
                    var identity = new ClaimsIdentity(new[]
                    {
                    new Claim(ClaimTypes.Name, userState.UserName),
                    new Claim(ClaimTypes.NameIdentifier, userState.UserID),
                    new Claim(ClaimTypes.Email, userState.Email ?? ""),
                    new Claim("Level", userState.Level.ToString()),
                    new Claim("Post", userState.Post),
                    new Claim("IsAdmin", userState.IsAdmin.ToString()),
                    new Claim("Division", userState.Division ?? ""),
                    }, CN.Setting.AuthenticationCookieName);
                    context.User = new ClaimsPrincipal(identity);
                }


            }

            await next(context);
        });

        return app;
    }
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

            // Use an inline delegate to pull the Scoped provider out of the active HttpContext
            options.RequestCultureProviders.Insert(0, new CustomRequestCultureProvider(async httpContext =>
            {
                // Resolves the provider dynamically within the active request context,
                // ensuring the Session middleware has already completed execution.
                var scopedProvider = httpContext.RequestServices.GetRequiredService<SessionCultureProvider>();
                return await scopedProvider.DetermineProviderCultureResult(httpContext);
            }));

        });

        return services;
    }
}

