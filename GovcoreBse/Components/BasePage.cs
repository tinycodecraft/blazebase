using Microsoft.AspNetCore.Components;
using GovcoreBse.Control;
namespace GovcoreBse.Components;

public class BasePage: CoreCancellableComponent
{
    [Inject]
    protected HttpClient MyClient { get; set; } = default!;

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
