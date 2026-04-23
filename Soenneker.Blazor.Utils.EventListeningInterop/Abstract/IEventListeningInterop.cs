using System.Threading.Tasks;
using System.Threading;

namespace Soenneker.Blazor.Utils.EventListeningInterop.Abstract;

/// <summary>
/// A base type for use with Blazor interops that need to listen for events.
/// </summary>
public interface IEventListeningInterop
{
    /// <summary>
    /// Adds an event listener to the specified HTML element with the given ID.
    /// </summary>
    /// <param name="functionName">The name of the function to be used within JavaScript to add the event listener</param>
    /// <param name="elementId">The ID of the HTML element to attach the event listener to.</param>
    /// <param name="eventName">The name of the event to listen for.</param>
    /// <param name="dotNetCallback">The .NET callback function or object to handle the event.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    ValueTask AddEventListener(string functionName, string elementId, string eventName, object dotNetCallback, CancellationToken cancellationToken = default);
}
