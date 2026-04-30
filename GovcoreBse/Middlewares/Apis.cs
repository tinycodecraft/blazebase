using GovcoreBse.Store.Commands;

using Cortex.Mediator;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http.HttpResults;

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
            case CN.AutocompleteGroup.fileupload:
                builder.MapPost("/", UploadFile).Produces(200, typeof(object));
                break;
            case CN.AutocompleteGroup.fileremove:
                builder.MapPost("/", RemoveFile).Produces(200, typeof(object));
                break;


        }

        return builder;
    }

    internal static async Task<IResult> RemoveFile(IWebHostEnvironment env,ILogger<Program> logger, IOptions<PathSetting> setting,[FromForm] string uniqueFileId)
    {
        logger.LogDebug("the file remove api is called with uniqueFileId: {uniqueFileId}", uniqueFileId);
        if(string.IsNullOrEmpty(uniqueFileId))
        {
            logger.LogError("uniqueFileId is null or empty");
            return TypedResults.Ok(new { success = false });
        }
        var revertpath = Path.Combine(env.ContentRootPath, setting.Value.Temp, uniqueFileId);
        if(Directory.Exists(revertpath))
        {
            foreach(var file in Directory.GetFiles(revertpath))
            {
                try
                {
                    File.Delete(file);
                    logger.LogDebug("Deleted file: {file}", file);
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, "Error deleting file: {file}", file);
                }
            }

        }


        return TypedResults.Ok(true);
    }

    internal static async Task<IResult> UploadFile(IHttpContextAccessor accessor,IFormFileCollection files,IWebHostEnvironment env,ILogger<Program> logger,IOptions<PathSetting> setting)
    {
        logger.LogDebug("the file upload api is called");
        var id = "".RandomString(8);
        var upload = setting.Value.Temp;
        if(accessor== null || accessor.HttpContext == null || files==null || files.Count == 0)
        {
            logger.LogError("HttpContext or file is null");
            return TypedResults.Ok(new { id = string.Empty });
        }

        string? type = accessor.HttpContext.Request.Form[CN.Setting.FILEPOND_ATTCHTYPE];
        if(type==null)
        {
            logger.LogError("Attachment type is missing in the form data");
            return TypedResults.Ok(new { id = string.Empty });
        }

        var tempuploadpath =Path.Combine(env.ContentRootPath, upload, id);
        if(!Directory.Exists(tempuploadpath))
        {
            Directory.CreateDirectory(tempuploadpath);
        }
        var file = files[0];
        using var stream = File.Create(tempuploadpath + "\\" + file.FileName);
        await file.CopyToAsync(stream);

        id = $"{type}{CN.Setting.CONTENT_SEPARATOR}{id}";

        return TypedResults.Ok(new { id = id });
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

    internal static async Task<IResult> GetFile(IHttpContextAccessor accessor,IMediator commander,ILogger<Program> logger, IOptions<PathSetting> setting, long thumb, string filename)
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
                tmppath = result.Value.UploadFilePath.Replace("{0}",string.Join("\\",basepath));

            }
        }
        var basename = HelperS.GetBaseName(filename);
        var ext = Path.GetExtension(filename);
        filename = $"{basename}{ext}";

        // Open the file as a stream. Using FileShare.Read allows other processes to read it.
        // Setting useAsync: true is recommended for high-performance I/O.
        //TODO: Please using IDocItem to get file path first
        var fileStream = new FileStream(tmppath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);




        return Results.File(fileStream, contentType, filename);

        
    }
}
