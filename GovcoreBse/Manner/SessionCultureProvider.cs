using Microsoft.AspNetCore.Localization;

namespace GovcoreBse.Manner;

public class SessionCultureProvider: RequestCultureProvider
{
    public override async Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        

        var sessionCulture = httpContext.Session.GetString(SK.SESSION_CULTURE);
        if (string.IsNullOrEmpty(sessionCulture))
        {
            return await Task.FromResult<ProviderCultureResult?>(null);
            
        }
        return await Task.FromResult( new ProviderCultureResult(sessionCulture));
    }
}
