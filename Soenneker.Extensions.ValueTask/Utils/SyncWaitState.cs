using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Soenneker.Extensions.ValueTask.Utils;

internal sealed class SyncWaitState
{
    private readonly ManualResetEventSlim _mres = new(false);
    private ValueTaskAwaiter _awaiter;
    private Exception? _exception;

    public SyncWaitState(ValueTaskAwaiter awaiter)
    {
        _awaiter = awaiter;
        _awaiter.UnsafeOnCompleted(Continue);
    }

    private void Continue()
    {
        try
        {
            _awaiter.GetResult();
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
    public void RethrowIfFaulted()
    {
        _mres.Dispose();

        if (_exception is not null)
            throw _exception;
    }
}