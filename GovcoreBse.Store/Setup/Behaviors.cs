using AgileObjects.AgileMapper;
using AgileObjects.AgileMapper.Api.Configuration;
using AgileObjects.AgileMapper.Configuration;
using Cortex.Mediator.Commands;
using Cortex.Mediator.Queries;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace GovcoreBse.Store.Setup;


public static class MapperBehaviors
{
    public static T AsEmptyWhenNull<T>(this T source)
    {
        Mapper.Map(source).Over(source);
        return source;
    }
    
    public static MappingConfigStartingPoint NullStringToEmpty<T>(this MappingConfigStartingPoint mapper,params Expression<Func<T, string>>[] stringProperties)
    {
        var config = mapper.From<T>().To<T>();
        foreach(var rr in stringProperties)
        {
            var r = rr.Compile();
            config.Map((s, t) => r(t)==null ? "": r(t)).To(rr);
        }

        return mapper;
    }
}


public class LoggingPipelineBehavior<TRequest, TResponse> : IQueryPipelineBehavior<TRequest, TResponse> where TRequest : IQuery<TResponse>
{
    private readonly ILogger<LoggingPipelineBehavior<TRequest, TResponse>> _logger;

    public LoggingPipelineBehavior(ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        QueryHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        //Request
        _logger.LogInformation(
            "Handling {Name}. {@Date}",
            requestName,
            DateTime.UtcNow);

        var result = await next();

        //Response
        _logger.LogInformation(
            "CleanArchitecture Request: {Name} {@request}. {@Date}",
            requestName,
            request,
            DateTime.UtcNow);

        return result;
    }
}