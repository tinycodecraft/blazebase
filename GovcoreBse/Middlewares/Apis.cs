using GovcoreBse.Store.Commands;
using GovcoreBse.Store.Dtos;
using Cortex.Mediator;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace GovcoreBse.Middlewares;

public static class Apis
{
    public static RouteGroupBuilder MapApiFor(this RouteGroupBuilder builder, CN.AutocompleteGroup group)
    {
        switch (group)
        {
            case CN.AutocompleteGroup.streambyname:
                builder.MapGet("/{thumb:long}/{filename}", GetFile).Produces(200, typeof(HttpResponseMessage));
                break;
            case CN.AutocompleteGroup.suggests:
                builder.MapGet("/{userid}", GetAllSuggestions).Produces(200, typeof(KeyValuePair<string, string>[]));
                break;

            case CN.AutocompleteGroup.weathers:
                builder.MapGet("/all/{userid}", GetAllWeathers).Produces(200, typeof(List<WeatherForecastDto>));

                break;


        }

        return builder;
    }

    internal static async Task<IResult> GetAllSuggestions(IHttpContextAccessor accessor,IMediator commander, ILogger<Program> logger, string userid, [FromQuery(Name = "wanted")] string wanted, [FromQuery] string? search = null)
    {
        var wantedtype = HelperS.GetEnum<CN.AutoSuggestType>(wanted);
        var result = await commander.SendQueryAsync(new GetAutoCompleteQuery(wantedtype, userid, search));

        if (result.IsError)
        {
            logger.LogDebug(result.FirstError.Description);
            return TypedResults.Ok(new KeyValuePair<string, string>[] { });
        }

        return TypedResults.Ok(result.Value);
    }

    internal static async Task<IResult> GetAllWeathers(IHttpContextAccessor accessor, IMediator commander, ILogger<Program> logger, string userid, [FromQuery(Name = "start")] int? start = 1, [FromQuery(Name = "size")] int? size = 10, [FromQuery(Name = "total")] int? total = 100)
    {
        var result = await commander.SendQueryAsync(new GetWeatherForecastsQuery(total ?? 100, start ?? 1, total ?? 100));
        if(result == null || result.Count == 0)
        {
            logger.LogDebug("No weather data found for user " + userid);
            return TypedResults.Ok(new List<WeatherForecastDto>());
        }

        return TypedResults.Ok(result);
    }

    internal static async Task<HttpResponseMessage> GetFile(IHttpContextAccessor accessor,IMediator commander,ILogger<Program> logger, IOptions<PathSetting> setting, long thumb, string filename)
    {
        // This is a placeholder implementation. You would replace this with your actual file retrieval logic.



        var contentType = HelperS.GetFileType(filename);
        var isinline = HelperS.CanInline(filename);
        var basepath = setting.Value.Share;
        var tmppath = string.Empty;
        var result = await commander.SendQueryAsync(new GetFileQuery(thumb));

        if(!result.IsError)
        {
            if(!string.IsNullOrEmpty(result.Value.UploadFilePath))
            {
                tmppath = Path.Combine(result.Value.UploadFilePath.Replace("{0}",string.Join("\\",basepath));

            }
        }
        var basename = HelperS.GetBaseName(filename);
        var ext = Path.GetExtension(filename);
        filename = $"{basename}{ext}";

        // Open the file as a stream. Using FileShare.Read allows other processes to read it.
        // Setting useAsync: true is recommended for high-performance I/O.
        //TODO: Please using IDocItem to get file path first
        var fileStream = new FileStream(tmppath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            // StreamContent handles chunking by default (standard 4KB-10KB).
            Content = new StreamContent(fileStream)
        };
        



        // 設置 Content-Type 為 PDF
        response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        // 關鍵：設置 Content-Disposition 為 inline 以在瀏覽器中預覽，而非強制下載
        response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(isinline ? "inline": "attachment")
        {
            FileName = filename
        };
        response.Headers.TransferEncodingChunked = true;

        return response;
    }
}
