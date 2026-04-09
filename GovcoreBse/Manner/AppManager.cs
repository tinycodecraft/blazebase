namespace GovcoreBse.Manner;

public class AppManager
{
    IHttpContextAccessor http;
    public AppManager(IHttpContextAccessor accessor)
    {
        http = accessor;
    }

    public static double NowSeconds
    {
        get
        {
            return (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }
    }

    public bool ClearState()
    {
        if (http.HttpContext == null)
        {
            return false;
        }
        if (http.HttpContext.Request.Cookies.ContainsKey(Constants.Setting.AuthorizeCookieKey))
        {
            http.HttpContext.Response.Cookies.Delete(Constants.Setting.AuthorizeCookieKey);
            return true;
        }
        return false;
    }

    public bool SaveState(UserState userv)
    {
        if(http.HttpContext == null)
        {
            return false;
        }

        var cookievalue = JWTHelper.GenerateToken("4", userv);

        if(http.HttpContext.Request.Cookies.ContainsKey(Constants.Setting.AuthorizeCookieKey))
        {
            http.HttpContext.Response.Cookies.Delete(Constants.Setting.AuthorizeCookieKey);
        }

        http.HttpContext.Response.Cookies.Append(Constants.Setting.AuthorizeCookieKey, cookievalue, new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTimeOffset.UtcNow.AddDays(1),
            SameSite = SameSiteMode.Strict,
        });

        return true;
    }

    public UserState? UserState
    {
        get
        {
            if(http.HttpContext== null)
            {
                return null;
            }
            var allcookies = http.HttpContext.Request.Cookies;
            var cookie = allcookies[Constants.Setting.AuthorizeCookieKey];
            
            var tokenInfo = cookie ?? "";

            if (string.IsNullOrEmpty(tokenInfo))
                return null;

            var encodeTokenInfo = JWTHelper.GetDecodingToken(tokenInfo);
            if (string.IsNullOrEmpty(encodeTokenInfo))
            {
                ClearState();
                return null;
            }
            UserState userState = JWTHelper.DElize<UserState>(encodeTokenInfo);

            if (userState != null && NowSeconds > userState.exp)
            {
                // token expired
                return null;
            }

            return userState;
        }
    }
}
