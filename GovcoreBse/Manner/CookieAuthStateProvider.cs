using Cortex.Mediator;
using GovcoreBse.Store.Commands;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Security.Cryptography;

namespace GovcoreBse.Manner;

public class CookieAuthStateProvider: AuthenticationStateProvider
{
    private readonly AppManager appManager;
    private readonly ILogger<CookieAuthStateProvider> logger;
    private readonly IHttpContextAccessor accessor;
    private readonly IMediator commander;
    public CookieAuthStateProvider(AppManager manager,ILogger<CookieAuthStateProvider> mlogger,IHttpContextAccessor maccessor,IMediator cmd)
    {
        appManager = manager;
        logger = mlogger;
        accessor = maccessor;
        commander = cmd;
    }
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        try
        {
            
            var userState = appManager.UserState;

            if (userState != null)
            {
                var result = await  commander.SendQueryAsync(new GetRolesQuery(userState.UserID));
                var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, userState.UserName),
                    new Claim(ClaimTypes.NameIdentifier, userState.UserID),                    
                    new Claim(ClaimTypes.Email, userState.Email),
                    new Claim("Level", userState.Level.ToString()),
                    new Claim("Post", userState.Post),
                    new Claim("IsAdmin", userState.IsAdmin.ToString()),
                    new Claim("Division", userState.Division ),
                }, CN.Setting.AuthenticationCookieName);
                if(!result.IsError)
                {
                    foreach(var r in result.Value.RoleNames)
                    {
                        identity.AddClaim(new Claim( ClaimTypes.Role, r));
                    }
                }
                
                var user = new ClaimsPrincipal(identity);
               
                return new AuthenticationState(user);
            }
            else
            {
                
                return new AuthenticationState(anonymous);
            }
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error in GetAuthenticationStateAsync");
        }
        return new AuthenticationState(anonymous);
    }
}
