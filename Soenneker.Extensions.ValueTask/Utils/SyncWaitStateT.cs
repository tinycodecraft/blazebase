using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Soenneker.Extensions.ValueTask.Utils;

internal sealed class SyncWaitState<T>
{
    private readonly ManualResetEventSlim _mres = new(false);
    private ValueTaskAwaiter<T> _awaiter;
    private Exception? _exception;
    private T? _result;

    public SyncWaitState(ValueTaskAwaiter<T> awaiter)
    {
        _awaiter = awaiter;
        _awaiter.UnsafeOnCompleted(Continue);
    }

    private void Continue()
    {
        try
        {
            _result = _awaiter.GetResult();
        }
        catch (Exception ex)
        {
            _exception = ex;
        }
        finally
        {
            _mres.Set();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Wait(CancellationToken cancellationToken)
    {
        if (!cancellationToken.CanBeCanceled)
        {
            _mres.Wait();
            return;
        }

        _mres.Wait(cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetResultOrThrow()
    {
        _mres.Dispose();

        if (_exception is not null)
            throw _exception;

        return _result!;
    }
}