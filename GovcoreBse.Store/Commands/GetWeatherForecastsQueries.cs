
using GovcoreBse.Store.Setup;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cortex.Mediator.Queries;

namespace GovcoreBse.Store.Commands
{
    public record GetWeatherForecastsQuery(int Total, int Start = 1, int Size = 0) : IQuery<List<WeatherForecastDto>>;

    public class GetWeatherForcecastsQueryHandler: IQueryHandler<GetWeatherForecastsQuery, List<WeatherForecastDto>>
    {
        public readonly IBlazeLogDbContext context;
        

        public GetWeatherForcecastsQueryHandler(IBlazeLogDbContext ctx)
        {
            context = ctx;
            
        }


        public async Task<List<WeatherForecastDto>> Handle(GetWeatherForecastsQuery request, CancellationToken cancellationToken)
        {
            var rng = new Random();
            //no of record to be taken (page size)
            var size = request.Size == 0 ? CN.Setting.PageSize : request.Size;
            //no of record to be skipped 
            var start = request.Start == 0 ? CN.Setting.PageStart : request.Start;

            return await Task.FromResult(Enumerable.Range(start, request.Size == 0 ? 5 : request.Size).Select(index => new WeatherForecastDto
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            }).ToList());
        }
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

    }
}
