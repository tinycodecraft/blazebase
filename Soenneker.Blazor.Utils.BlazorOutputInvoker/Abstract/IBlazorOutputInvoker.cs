using System.Threading.Tasks;

namespace Soenneker.Blazor.Utils.BlazorOutputInvoker.Abstract;

/// <summary>
/// A generic invoker to simplify JavaScript to C# interaction that allows for an input and output, providing two-way communication with invocations.
/// </summary>
public interface IBlazorOutputInvoker<in TInput, TOutput>
{
    /// <summary>
    /// Invokes the Blazor invoker.
    /// </summary>
    /// <param name="args">The input argument.</param>
    /// <returns>A <see cref="ValueTask{TOutput}"/> representing the asynchronous operation and containing the output result.</returns>
    ValueTask<TOutput> InvokeWithOutput(TInput args);
}
