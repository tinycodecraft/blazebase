using GovcoreBse.Store.Commands;
using GovcoreBse.Store.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GovcoreBse.Middlewares;

public static class Apis
{
    public static RouteGroupBuilder MapApiFor(this RouteGroupBuilder builder, CN.AutocompleteGroup group)
    {
        switch (group)
        {
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
        var result = await commander.Send(new GetAutoCompleteQuery(wantedtype, userid, search));

        if (result.IsError)
        {
            logger.LogDebug(result.FirstError.Description);
            return TypedResults.Ok(new KeyValuePair<string, string>[] { });
        }

        return TypedResults.Ok(result.Value);
    }

    internal static async Task<IResult> GetAllWeathers(IHttpContextAccessor accessor, IMediator commander, ILogger<Program> logger, string userid, [FromQuery(Name = "start")] int? start = 1, [FromQuery(Name = "size")] int? size = 10, [FromQuery(Name = "total")] int? total = 100)
    {
        var result = await commander.Send(new GetWeatherForecastsQuery(total ?? 100, start ?? 1, total ?? 100));
        if(result == null || result.Count == 0)
        {
            logger.LogDebug("No weather data found for user " + userid);
            return TypedResults.Ok(new List<WeatherForecastDto>());
        }

        return TypedResults.Ok(result);
    }
}
