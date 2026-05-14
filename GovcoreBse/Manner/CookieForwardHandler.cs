namespace GovcoreBse.Manner;

public class CookieForwardHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    

    public CookieForwardHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var cookieHeaders = new List<string>();

        
        // Extract the cookie from the incoming browser request
        if (httpContext != null && httpContext.Request.Cookies.TryGetValue(CN.Setting.AuthorizeCookieKey, out var cookieValue))
        {
            cookieHeaders.Add($"{CN.Setting.AuthorizeCookieKey}={cookieValue}");
            // Attach the exact same encrypted cookie value to the outbound HttpClient request
            
        }
        if (httpContext != null && httpContext.Request.Cookies.TryGetValue(CN.Setting.AntiForgeryCookieKey, out var forgValue))
        {
            cookieHeaders.Add($"{CN.Setting.AntiForgeryCookieKey}={forgValue}");
        }

        if (cookieHeaders.Any())
        {
            request.Headers.Add("Cookie", string.Join("; ", cookieHeaders));
        }

        return await base.SendAsync(request, cancellationToken);
    }
}