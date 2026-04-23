using System;
using System.Runtime.CompilerServices;

namespace Soenneker.Extensions.ValueTask.Utils;

internal sealed class ObserveState
{
    private ValueTaskAwaiter _awaiter;
    private readonly Action<Exception>? _handler;

    public ObserveState(ValueTaskAwaiter awaiter, Action<Exception>? handler)
    {
        _awaiter = awaiter;
        _handler = handler;
    }

    public void Continue()
    {
        try
        {
            _awaiter.GetResult();
        }
        catch (Exception ex)
        {
            _handler?.Invoke(ex);
        }
    }
}