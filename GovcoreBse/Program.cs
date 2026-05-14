using GovcoreBse.Components;
using GovcoreBse.Control;
using GovcoreBse.Manner;
using GovcoreBse.Middlewares;
using GovcoreBse.Models;
using GovcoreBse.Resources;
using GovcoreBse.Store.Setup;
using Havit.Blazor.Components.Web;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Localization;
using Serilog;
using Soenneker.Blazor.FilePond.Registrars;
using System.Text.Json.Serialization;


/*
 * Please study 
 * https://www.fluentui-blazor.net/
 * https://github.com/microsoft/fluentui-blazor
 * for using blazor easily
 */

/*Bootstrap logger
 */
Log.Logger = new LoggerConfiguration().MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ApplicationName = typeof(Program).Assembly.GetName().Name,
    ContentRootPath = Directory.GetCurrentDirectory(),
    //WebRootPath = "wwwroot",
    

});

var baseurl = builder.WebHost.GetSetting(WebHostDefaults.ServerUrlsKey);

//Setting configurations
var authsetting = builder.Configuration.GetSection(CN.Setting.AuthSetting);
builder.Services.Configure<AuthSetting>(authsetting);
var dbrcusetting = builder.Configuration.GetSection(CN.Setting.DBRCUSetting);
builder.Services.Configure<DBRCUSetting>(dbrcusetting);
var pathsetting = builder.Configuration.GetSection(CN.Setting.PathSetting);
builder.Services.Configure<PathSetting>(pathsetting);

builder.Services.Configure<FormOptions>(opt =>
{

    opt.BufferBodyLengthLimit = 512 * 1024 * 1024;

    //it needs
    opt.MultipartBodyLengthLimit = 512 * 1024 * 1024;

});

builder.Services.Configure<IISServerOptions>(opt =>
{
    opt.MaxRequestBodySize = 512 * 1024 * 1024;

});
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

builder.Services.AddSingleton<AppManager>();
builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthStateProvider>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.Cookie.Name = CN.Setting.AuthorizeCookieKey;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Events.OnRedirectToLogin = context =>
        {
            // Crucial: Stops Minimal APIs from returning 302 Redirect to a login HTML page.
            // Instead, it returns a clean 413 or 401 Unauthorized API error status.
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();
//add common service
builder.Services.AddScoped<IN.ITokenService,TokenService>();

//Add razor view global state
//add the state as signleton is wrong because it should depends user session instead
builder.Services.AddScoped<LayoutStateModel>();
//Add razor Js module 
builder.Services.AddScoped<ExampleJsInterop>();

builder.Services.AddScoped<StringEncrypService>();
builder.Services.AddTransient<CookieForwardHandler>();
builder.Services.AddHttpClient(CN.Setting.AutoApiClientName).AddHttpMessageHandler<CookieForwardHandler>();

builder.Services.AddFilePondInteropAsScoped();

builder.Services.AddScoped<IStringLocalizer>(provider =>
{
    var stringFactory = provider.GetRequiredService<IStringLocalizerFactory>();
    return stringFactory.Create(typeof(GovcoreBse.Resources.SharedResource).Name, typeof(Program).Assembly.GetName().Name!);

    
});

/*UseSerilog configuration
 */
builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
.ReadFrom.Configuration(context.Configuration)
.ReadFrom.Services(services)
//.WriteTo.Console(new ExpressionTemplate(
//    // Include trace and span ids when present.
//    "[{@t:HH:mm:ss} {@l:u3}{#if @tr is not null} ({substring(@tr,0,4)}:{substring(@sp,0,4)}){#end}] {@m}\n{@x}"))
//
);

builder.Services.AddTransient<ProblemDetailsFactory, CustomProblemDetailsFactory>();


builder.Services.AddCommandMapper();

builder.Services.AddStore<DBRCUSetting>();



builder.Services.AddControllersWithViews()
    .AddJsonOptions(opt => opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles)
    .AddViewLocalization( LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization(options => {
        //using same resource for data annotation for multiple classes
        options.DataAnnotationLocalizerProvider = (type, factory) =>
        factory.Create(typeof(SharedResource));
    });


builder.Services
.AddRazorComponents()
.AddInteractiveServerComponents()
.AddCircuitOptions(options => options.DetailedErrors = true); // for debugging razor components

builder.WebHost.UseStaticWebAssets();

builder.Services.AddScoped<IUrlHelper>(sp => sp.GetRequiredService<IUrlHelperFactory>().GetUrlHelper(sp.GetRequiredService<IActionContextAccessor>().ActionContext));

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = CN.Setting.SessionCookieKey;
});
// Add services to the container.
//builder.Services.AddControllersWithViews();
// 2. Register your custom provider class as a SCOPED service
builder.Services.AddScoped<SessionCultureProvider>();

builder.Services.AddCustomLocalization(DK.LANG_ENG, DK.LANG_CHI);

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = CN.Setting.AntiForgeryCookieKey; // 自訂 Cookie 名稱
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // 僅限 HTTPS
    options.Cookie.SameSite = SameSiteMode.Strict; // 強安全模式
    options.Cookie.HttpOnly = true; // 防止 JS 讀取
    options.Cookie.IsEssential = true; // 視為必要 Cookie
    options.HeaderName = CN.Setting.AntiForgeryId;

});

builder.Services.AddHxServices();
builder.Services.AddHxMessenger();
builder.Services.AddHxMessageBoxHost();


var app = builder.Build();


app.UseClaimsIdentityResolver();


app.UseApiExceptionHandling();

app.MapGet("/api/hello", () => "Hello, World!");
//test successful
//app.MapGet("/api/throw", () => { throw new Exception("This is a test exception"); return Results.Ok("Not Ok"); });

app.MapGroup("/api/" + nameof(CN.AutocompleteGroup.weathers))
    .MapApiFor(CN.AutocompleteGroup.weathers)
    .WithTags(nameof(CN.AutocompleteGroup.weathers));

app.MapGroup("/api/" + nameof(CN.AutocompleteGroup.suggests))
    .MapApiFor(CN.AutocompleteGroup.suggests)
    .WithTags(nameof(CN.AutocompleteGroup.suggests));

app.MapGroup("/api/" + nameof(CN.AutocompleteGroup.streambyname))
    .MapApiFor(CN.AutocompleteGroup.streambyname)
    .WithTags(nameof(CN.AutocompleteGroup.streambyname));

app.MapGroup("/api/" + nameof(CN.AutocompleteGroup.fileupload))
    .MapApiFor(CN.AutocompleteGroup.fileupload)
    .WithTags(nameof(CN.AutocompleteGroup.fileupload));

app.MapGroup("/api/" + nameof(CN.AutocompleteGroup.culture))
    .MapApiFor(CN.AutocompleteGroup.culture)
    .WithTags(nameof(CN.AutocompleteGroup.culture));


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

/*Use SerilogRequestLogging
 */
app.UseSerilogRequestLogging(option =>
{
    option.EnrichDiagnosticContext = (diagnostic, http) =>
    {
        diagnostic.Set("LocalTime", DateTime.Now.ToString("yyyyMMdd+HHmmss"));

    };
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
//Please don't apply to "ApplyCurrentCultureToResponseHeaders"
//otherwise, the cookie localization not work
app.UseRequestLocalization();

//**this is for razor components
//**not just element for mvc
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorComponents<App>()
.AddInteractiveServerRenderMode();

app.Run();
