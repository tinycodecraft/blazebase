using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.JSInterop;
using Microsoft.VisualBasic;
using CN=GovcoreBse.Common.Constants;

namespace GovcoreBse.Control;

// This class provides an example of how JavaScript functionality can be wrapped
// in a .NET class for easy consumption. The associated JavaScript module is
// loaded on demand when first needed.
//
// This class can be registered as scoped DI service and then injected into Blazor
// components for use.

public class ExampleJsInterop : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;
    private readonly ILogger<ExampleJsInterop> mylogger;

    public ExampleJsInterop(IJSRuntime jsRuntime,ILogger<ExampleJsInterop> logger)
    {
        moduleTask = new (() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/GovcoreBse.Control/js/exampleJsInterop.js?v=1").AsTask());
        mylogger = logger;
    }

    public async ValueTask<string> Prompt(string message)
    {
        var module = await moduleTask.Value;
        return await module.InvokeAsync<string>("showPrompt", message);
    }

    public async ValueTask SaveUrl(string? urlHistoryKey=null)
    {
        urlHistoryKey ??= CN.Setting.UrlHistoryKey;
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("saveUrlHistory", urlHistoryKey);
    }

    public async ValueTask<string> GetPreviousUrl(string? urlHistoryKey=null)
    {
        urlHistoryKey ??= CN.Setting.UrlHistoryKey;
        var module = await moduleTask.Value;
        return await module.InvokeAsync<string>("getUrlHistory", urlHistoryKey);
    }

    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            try
            {
                await module.InvokeVoidAsync("dispose");
            }
            catch (Exception ex) {
                mylogger.LogDebug($"trying to dispose script ExampleJsInterop with error: {ex.Message}");

            }
            await module.DisposeAsync();
        }
    }
}
