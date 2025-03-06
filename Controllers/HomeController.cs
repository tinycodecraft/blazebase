using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using blazelogBase.Models;
using blazelogBase.Middlewares;
using blazelogBase.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Localization;

namespace blazelogBase.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IStringLocalizer<blazelogBase.SharedResource> _stringLocalizer;

    public HomeController(ILogger<HomeController> logger,IStringLocalizer<blazelogBase.SharedResource> localizer)
    {
        _logger = logger;
        _stringLocalizer = localizer;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        string? cultureCookieValue = null;
        this.HttpContext.Request.Cookies.TryGetValue(
            CookieRequestCultureProvider.DefaultCookieName, out cultureCookieValue);


        var model = ViewModelFactory.CreateViewModelWithResource<PrivacyViewModel,blazelogBase.SharedResource>(_stringLocalizer);
        string text = "Thread CurrentUICulture is [" + @Thread.CurrentThread.CurrentUICulture.ToString() + "] ; ";
        text += "Thread CurrentCulture is [" + @Thread.CurrentThread.CurrentCulture.ToString() + "]";

        model.Culture = text;

        return View(model);
    }

    public IActionResult ChangeLang(ChangeLangModel model)
    {
        if(model.IsSubmit)
        {
            this.HttpContext.SetLangCookie(model.SelectedLanguage,year:1,day:0);

            return LocalRedirect("/");

        }
        model = ViewModelFactory.CreateChangeLangModel();

        return View(model);
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
