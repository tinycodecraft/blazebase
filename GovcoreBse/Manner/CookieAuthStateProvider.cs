using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace GovcoreBse.Manner;

public class CookieAuthStateProvider: AuthenticationStateProvider
{
    private readonly AppManager appManager;
    private readonly ILogger<CookieAuthStateProvider> logger;
    private readonly IHttpContextAccessor accessor;
    public CookieAuthStateProvider(AppManager manager,ILogger<CookieAuthStateProvider> mlogger,IHttpContextAccessor maccessor)
    {
        appManager = manager;
        logger = mlogger;
        accessor = maccessor;
    }
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        try
        {

            var userState = appManager.UserState;

            if (userState != null)
            {
                var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, userState.UserName),
                    new Claim(ClaimTypes.NameIdentifier, userState.UserID),                    
                    new Claim(ClaimTypes.Email, userState.Email),
                    new Claim("Level", userState.Level.ToString()),
                    new Claim("Post", userState.Post),
                    new Claim("IsAdmin", userState.IsAdmin.ToString()),
                    new Claim("Division", userState.Division ),
                }, "CookieAuth");
                var user = new ClaimsPrincipal(identity);
                return Task.FromResult(new AuthenticationState(user));
            }
            else
            {
                
                return Task.FromResult(new AuthenticationState(anonymous));
            }
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error in GetAuthenticationStateAsync");
        }
        return Task.FromResult(new AuthenticationState(anonymous));
    }
}
