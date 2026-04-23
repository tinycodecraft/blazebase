using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Utils.File.Registrars;
using Soenneker.Utils.Json.Abstract;

namespace Soenneker.Utils.Json.Registrars;

/// <summary>
/// A utility library handling (de)serialization and other useful JSON functionalities
/// </summary>
public static class JsonUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IJsonUtil"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddJsonUtilAsSingleton(this IServiceCollection services)
    {
        services.AddFileUtilAsSingleton()
                .TryAddSingleton<IJsonUtil, JsonUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IJsonUtil"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddJsonUtilAsScoped(this IServiceCollection services)
    {
        services.AddFileUtilAsScoped()
                .TryAddScoped<IJsonUtil, JsonUtil>();

        return services;
    }
}
