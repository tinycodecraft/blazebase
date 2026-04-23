using Soenneker.Blazor.Utils.BlazorOutputInvoker.Abstract;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System;
using Microsoft.JSInterop;

namespace Soenneker.Blazor.Utils.BlazorOutputInvoker;

///<inheritdoc cref="IBlazorOutputInvoker{TInput,TOutput}"/>
public sealed class BlazorOutputInvoker<TInput, TOutput> : IBlazorOutputInvoker<TInput, TOutput>
{
    private readonly Func<TInput, ValueTask<TOutput>> _func;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlazorOutputInvoker{TInput,TOutput}"/> class.
    /// </summary>
    /// <param name="invoker">The invoker function.</param>
    [DynamicDependency(nameof(InvokeWithOutput))]
    public BlazorOutputInvoker(Func<TInput, ValueTask<TOutput>> invoker)
    {
        _func = invoker;
    }

    // CancellationToken cannot be a parameter due to this being called from JS
    [JSInvokable(nameof(InvokeWithOutput))]
    public ValueTask<TOutput> InvokeWithOutput(TInput args)
    {
        return _func(args);
    }
}
