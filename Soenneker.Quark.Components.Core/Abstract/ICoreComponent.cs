using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soenneker.Quark;

/// <summary>
/// A Blazor core class for the Quark component.
/// </summary>
public interface ICoreComponent : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets or sets the HTML attributes to apply to the element.
    /// </summary>
    IReadOnlyDictionary<string, object>? Attributes { get; set; }

    string? Id { get; set; }

    /// <summary>
    /// Disposes managed resources for the component. Implementations should be idempotent.
    /// </summary>
    new void Dispose();

    /// <summary>
    /// Asynchronously disposes managed resources for the component. Implementations should be idempotent.
    /// </summary>
    /// <returns>A task that completes when asynchronous disposal is finished.</returns>
    new ValueTask DisposeAsync();
}
