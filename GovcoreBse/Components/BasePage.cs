using Microsoft.AspNetCore.Components;
using GovcoreBse.Control;
namespace GovcoreBse.Components;

public class BasePage: CoreCancellableComponent
{
    [Inject]
    protected HttpClient MyClient { get; set; } = default!;

    public virtual async ValueTask<FN.IFilePondLoadRequest?> OnFilePondRemoveFile(FN.IFilePondLoadRequest? request=null, CancellationToken cancellationToken = default)
    {
        if(request!=null && !string.IsNullOrEmpty(request.Source) && request.Source.Contains("|"))
        {
            var valparts = request.Source.ItSplit("|").ToList();
            var replace= string.Empty;
            var foundi = -1;
            for (int i=0; i < request.InUrls.Length; i++) {
                if (request.InUrls[i]== request.Source)
                {
                    replace = $"-|{valparts[1]}";
                    foundi = i;
                    break;
                }
                if(foundi >=0)
                {
                    
                    request.OutUrls= request.InUrls.Where((url, index) => index != foundi).ToArray().Union(new[] { replace }).ToArray();
                }
            }

        }
        else if(request!=null && !string.IsNullOrEmpty(request.Source))
        {
            request.OutUrls = request.InUrls.Where(url => !string.Equals(url, request.Source, StringComparison.OrdinalIgnoreCase)).ToArray();
        }
        return request;
    }

    public virtual async ValueTask<FN.IFilePondLoadRequest?> OnFilePondLoadFile(FN.IFilePondLoadRequest? request=null, CancellationToken cancellationToken = default)
    {
        

        if (request!=null && !string.IsNullOrEmpty(request.Source))
        {
            if(request.InUrls== null || !request.InUrls.Contains(request.Source, StringComparer.OrdinalIgnoreCase ))
            {
                request.OutUrls ??= new string[] { };
                request.OutUrls = request.OutUrls.Union(new[] { request.Source }).ToArray();
            }
        }
        return request;
    }
}
