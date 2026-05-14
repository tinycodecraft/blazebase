using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GovcoreBse.Models;
using GovcoreBse.Middlewares;
using GovcoreBse.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Localization;

using GovcoreBse.Store.Commands;


using GovcoreBse.Components.Pages;
using GovcoreBse.Shared.Tools;

using GovcoreBse.Manner;
using Cortex.Mediator;
using Mapster;
using GovcoreBse.Store.Setup;


namespace GovcoreBse.Controllers;

public class HomeController : Controller
{
    private readonly IMediator commander;
    private readonly ILogger<HomeController> logger;
    private readonly IStringLocalizer _stringLocalizer;
    private readonly IN.ITokenService tokener;
    private readonly LayoutStateModel globalState;
    private readonly ISession? session;
    private readonly AppManager manner;

    public HomeController(ILogger<HomeController> mlogger,IStringLocalizer localizer ,IMediator mediator,IN.ITokenService tokenHelper, IHttpContextAccessor accessor,AppManager appManager,LayoutStateModel layoutstate)
    {
        logger = mlogger;
        //using Factory instead of Dummy type GovcoreBse.SharedResource as generic type of IStringLocalizer<>
        _stringLocalizer = localizer;
        commander = mediator;
        tokener = tokenHelper;
        globalState = layoutstate;
        manner = appManager;
        logger.LogDebug("HomeController created");
        var sessionId = accessor.HttpContext?.Session.Id;
        logger.LogDebug("try to get session : " + sessionId);
        if(!string.IsNullOrEmpty(sessionId))
        {
            session = accessor.HttpContext!.Session;
        }
        
    }
    [HttpGet]
    public async Task<IActionResult> Index(bool needClear=false,string? returnUrl=null)
    {

        if(needClear)
        {
            manner.ClearState();
        }
        ViewBag.ReturnUrl = returnUrl;


        return View(new LoginModel());

    }

    [HttpPost]
    public async Task<IActionResult> Index(LoginModel model)
    {
        if(!ModelState.IsValid)
        {
            logger.LogDebug("ModelState is not valid");                
            return View(model);
        }
        if (session != null && !string.IsNullOrEmpty(model.UserId))
        {
            var userid = model.UserId;
            session.SetString(SK.SESSION_USERID, model.UserId);
            var result =await commander.SendQueryAsync(new GetUserQuery(model.UserId));
            
            if(result.IsError)
            {
                logger.LogDebug("User not found for userId : " + model.UserId);
                return View(model);
            }
            
            var user = result.Value.Adapt<UserState>().AsEmptyWhenNull();

            if(!manner.SaveState(user))
            {

                logger.LogDebug(user.UserName + " state could not be saved to cookie");

                return View(model);
            }
            globalState.CurrentCulture = System.Globalization.CultureInfo.CurrentCulture.Name;
            globalState.UserName= user.UserName;
            globalState.IsAdmin = user.IsAdmin;
            globalState.Post = user.Post;
           
        }

        return RedirectToAction("Welcome");
    }
    public IActionResult Welcome()
    {
        if (manner.UserState != null && manner.UserState.UserName != null)
        {
            ViewBag.LoginState = true;
            ViewBag.UserName = manner.UserState.UserName;
            ViewBag.UserPost = manner.UserState.Post;
        }
            

        return View(globalState);
    }



    public IActionResult Weather(int total =5000)
    {

        return View(new GetWeatherForecastsQuery(total,0, 19));

    }


    public IActionResult Privacy()
    {
        string? cultureCookieValue = null;
        this.HttpContext.Request.Cookies.TryGetValue(
            CookieRequestCultureProvider.DefaultCookieName, out cultureCookieValue);

        //please note the currentthread.currentthread.currentuiculture is automatically using underlining culture provider if available
        var model = ViewModelFactory.CreateViewModelWithResource<PrivacyViewModel>(_stringLocalizer);
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

#region Other examples


//public IResult Login()
//{
//    return this.RazorView<Login>();
//}
//The Sample has problem because accessor could not be injected
#endregion