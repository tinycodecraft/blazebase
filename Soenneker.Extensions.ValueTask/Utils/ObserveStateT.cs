using System;
using System.Runtime.CompilerServices;

namespace Soenneker.Extensions.ValueTask.Utils;

internal sealed class ObserveState<T>
{
    private ValueTaskAwaiter<T> _awaiter;
    private readonly Action<Exception>? _handler;

    public ObserveState(ValueTaskAwaiter<T> awaiter, Action<Exception>? handler)
    {
        _awaiter = awaiter;
        _handler = handler;
    }

    public void Continue()
    {
        try
        {
            _ = _awaiter.GetResult();
        }
        catch (Exception ex)
        {
            _handler?.Invoke(ex);
        }
    }
}