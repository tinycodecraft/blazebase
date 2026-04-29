
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GovcoreBse.Common;

public class Interfaces
{


    /// <summary>
    /// Contract for components that expose a cancellable, resettable async work scope.
    /// </summary>
    public interface ICoreCancellableComponent : ICoreComponent
    {
        /// <summary>
        /// Gets the current token for in-flight work.
        /// </summary>
        /// <remarks>
        /// Should return <see cref="CancellationToken.None"/> after disposal.
        /// </remarks>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Cancels any in-flight work. No-op if nothing has started.
        /// </summary>
        Task Cancel();

        /// <summary>
        /// Cancels current work and swaps in a fresh token/source for new work.
        /// </summary>
        ValueTask ResetCancellation();
    }


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


    /// <summary>
    /// Thread-safe holder for a single resource that can be lazily created,
    /// atomically reset (swap), and asynchronously torn down.
    /// </summary>
    /// <typeparam name="T">The resource type being managed.</typeparam>
    public interface IAtomicResource<out T> : IAsyncDisposable, IDisposable where T : class
    {
        /// <summary>
        /// Gets the current instance, creating it if necessary.
        /// </summary>
        /// <remarks>
        /// Implementations should be safe for concurrent callers and avoid
        /// duplicate allocations (i.e., publish-at-most-once semantics per reset).
        /// If the resource has been disposed, this should return <c>null</c>.
        /// </remarks>
        /// <returns>The current instance, or <c>null</c> if disposed.</returns>
        T? GetOrCreate();

        /// <summary>
        /// Returns the current instance if present, without creating a new one.
        /// </summary>
        /// <returns>The existing instance, or <c>null</c> if none has been created or the resource is disposed.</returns>
        T? TryGet();

        /// <summary>
        /// Atomically replaces the current instance with a freshly created one,
        /// and asynchronously tears down the previous instance (if any).
        /// </summary>
        /// <remarks>
        /// After this completes, subsequent <see cref="GetOrCreate"/> calls should return the new instance.
        /// </remarks>
        ValueTask Reset();

        /// <summary>
        /// Indicates whether the resource has been disposed and will no longer create or return instances.
        /// </summary>
        bool IsDisposed { get; }
    }



    public interface ITokenService
    {
        string CreateToken(IAuthResult user);
        IAuthResult? DecodeTokenToUser(string token);
    }

    //this interface is for mediatr , which require connectionid provided during subscription of signalr event

    //the detail can be found in vtecore, WeatherForcastHandler
    //1. handler is inited by mediatr for handling the request
    //2. handler will be injected with gateway (channel) to deliver the message
    //3. gateway has list of observer, the signalr client send signal to hub and create subscriber with (id,method and client proxy) and using the gateway for subscription

    //not using here for HyD project
    //the type argument in the interface is data type to be requested
    //a concrete interface (non-generic) is also required for mediatr to work
    //public interface IRqBase<T> : IRequest<ErrorOr<T>>
    //{
    //    string ConnectionId { get; set; }
    //}

    public interface IResultGateway<T> : IObservable<KeyValuePair<string, T>>
    {
        Task OnDeliverResultAsync(KeyValuePair<string, T> result);
    }

    public interface ILanguageService
    {
        public string LanguageId { get; }
    }
    public interface IBelongtoTable
    {
        string tablename { get; set; }
    }

    public interface IFileService
    {
        string CreatePathFor(string type, string filename, bool inupload = false);
        string GenerateWordWithData(string xmldata, string templatename, string type = null);
        Task<string> DownloadFilesAsync(Stream fileStream, string type, string filename, bool inupload = false);
        Task<FileUploadSummary> UploadFileAsync(Stream fileStream, string contentType, string type);
    }

  
    public interface IAuthResult
    {
        double exp { get; set; }
        string UserName { get; set; }
        string UserID { get; set; }
        int Level { get; set; }
        bool IsAdmin { get; set; }
        string Post { get; set; }
        string Division { get; set; }
        string Email { get; set; }

    }

    public interface IDBSetting
    {
        string DBsource { get; set; }
        string DBcatalog { get; set; }
        string DBuser { get; set; }
        string DBpwd { get; set; }
    }

    public interface IKeep
    {
        int DocKeep { get; set; }
    }

    public interface IUniqueItem
    {
        int Id { get; set; }
    }
    public interface IDocItem
    {
        long Id { get; set; }
        string DocType { get; set; }
        string UploadFilePath { get; set; }
        string RelativePath { get; set; }
    }


    
}