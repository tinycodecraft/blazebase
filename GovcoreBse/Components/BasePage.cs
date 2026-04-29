using Microsoft.AspNetCore.Components;
using GovcoreBse.Control;
namespace GovcoreBse.Components;

public class BasePage: CoreCancellableComponent
{
    [Inject]
    protected HttpClient MyClient { get; set; } = default!;


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
