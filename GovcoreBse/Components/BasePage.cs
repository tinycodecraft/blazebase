using Cortex.Mediator;
using DocumentFormat.OpenXml.Wordprocessing;
using GovcoreBse.Control;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace GovcoreBse.Components;

public class BasePage: CoreCancellableComponent
{

    [Inject]
    protected IMediator Commander { get; set; } = default!;
    [Inject]
    protected IOptions<PathSetting> Settings { get;set; } = default!;

    [Inject]
    protected Microsoft.AspNetCore.Antiforgery.IAntiforgery Antiforgery { get; set; }= default!;
    [Inject]
    protected HttpClient MyClient { get; set; } = default!;
    [Inject]
    protected IHttpContextAccessor Accessor { get; set; }= default!;
    protected string? GetToken()
    {
        var context = Accessor?.HttpContext;
        if (context != null)
        {
            return Antiforgery.GetAndStoreTokens(context).RequestToken;
        }
        return null;
    }

    public virtual async ValueTask<FN.IFilePondLoadRequest> OnFilePondRemoveFile(FN.IFilePondLoadRequest request, CancellationToken cancellationToken )
    {
        if(request!=null && !string.IsNullOrEmpty(request.Source) && request.Source.Contains("|"))
        {
            var valparts = request.Source.ItSplit("|").ToList();
            var replace= string.Empty;
            var foundi = -1;
            for (int i=0; i < request.Urls.Length; i++) {
                if (request.Urls[i]== request.Source)
                {
                    replace = $"-|{valparts[1]}";
                    foundi = i;
                    break;
                }

            }

            if (foundi >= 0)
            {

                request.Urls = request.Urls.Where((url, index) => index != foundi).ToArray().Union(new[] { replace }).ToArray();
            }

        }
        else if(request!=null && !string.IsNullOrEmpty(request.Source))
        {
            request.Urls = request.Urls.Where(url => !string.Equals(url, request.Source, StringComparison.OrdinalIgnoreCase)).ToArray();
        }
        return request!;
    }

    public virtual async ValueTask<FN.IFilePondLoadRequest> OnFilePondLoadFile(FN.IFilePondLoadRequest request, CancellationToken cancellationToken)
    {
        if (request!=null && !string.IsNullOrEmpty(request.Source))
        {
            if(request.Urls== null || !request.Urls.Contains(request.Source, StringComparer.OrdinalIgnoreCase ))
            {
                request.Urls ??= new string[] { };
                request.Urls = request.Urls.Union(new[] { request.Source }).ToArray();
            }
        }
        return request!;
    }
}
