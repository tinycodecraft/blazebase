
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GovcoreBse.Common;

public class Interfaces
{

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
}