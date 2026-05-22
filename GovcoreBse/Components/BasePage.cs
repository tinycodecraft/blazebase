using Cortex.Mediator;
using GovcoreBse.Control;
using GovcoreBse.Manner;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace GovcoreBse.Components;

public class BasePage: CoreCancellableComponent
{

    [CascadingParameter(Name ="PreviousLocation")]
    protected string ReferrerUrl { get; set; }
    // usse for redirecting to other page, or get current url
    // i.e. Navmanner.NavigateTo("login", forceLoad: true);
    [CascadingParameter(Name = "CurrentLocation")]
    protected string PageUrl { get; set; }

    [Inject]
    protected IJSRuntime Js { get; set; } = default!;

    [Inject]    
    protected NavigationManager Navmanner { get; set; }= default!;

    [Inject]
    protected IMediator Commander { get; set; } = default!;
    [Inject]
    protected IOptions<PathSetting> Settings { get;set; } = default!;

    [Inject]
    protected AntiforgeryStateProvider Antiforgery { get; set; }= default!;
    [Inject]
    protected AppManager Manner { get; set; }

    protected string? GetToken()
    {
        return Antiforgery.GetAntiforgeryToken()?.Value;

    }

    protected string? GetUserId()
    {
        return Manner?.UserState?.UserID;
    }


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if(firstRender)
        {
            PageUrl = Navmanner.Uri;
            var curresult = await Js.InvokeAsync<string>("eval", "window.location.href");
            // Invoke the browser's native document.referrer API
            var result = await Js.InvokeAsync<string>("eval", "history.state?.prevUrl ?? document.referrer");
            if (!string.IsNullOrEmpty(result))
            {
                ReferrerUrl = result;
            }

            StateHasChanged();
        }

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
