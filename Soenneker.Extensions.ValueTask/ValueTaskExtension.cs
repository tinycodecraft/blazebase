using Soenneker.Extensions.ValueTask.Utils;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Extensions.ValueTask;

/// <summary>
/// A collection of high-performance extension methods for working with <see cref="ValueTask"/> and <see cref="ValueTask{TResult}"/>.
/// These helpers focus on avoiding synchronization-context capture, minimizing allocations, and safely bridging
/// asynchronous code into synchronous execution when required.
/// </summary>
public static class ValueTaskExtension
{
    /// <summary>
    /// Configures an awaiter for the specified <see cref="ValueTask"/> that does not capture
    /// the current synchronization context.
    /// Equivalent to calling <c>ConfigureAwait(false)</c>.
    /// </summary>
    /// <param name="valueTask">The <see cref="ValueTask"/> to configure.</param>
    /// <returns>A configured awaitable that will not resume on the captured context.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConfiguredValueTaskAwaitable NoSync(this System.Threading.Tasks.ValueTask valueTask) => valueTask.ConfigureAwait(false);

    /// <summary>
    /// Configures an awaiter for the specified <see cref="ValueTask{TResult}"/> that does not capture
    /// the current synchronization context.
    /// Equivalent to calling <c>ConfigureAwait(false)</c>.
    /// </summary>
    /// <typeparam name="T">The result type of the <see cref="ValueTask{TResult}"/>.</typeparam>
    /// <param name="valueTask">The <see cref="ValueTask{TResult}"/> to configure.</param>
    /// <returns>A configured awaitable that will not resume on the captured context.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConfiguredValueTaskAwaitable<T> NoSync<T>(this ValueTask<T> valueTask) => valueTask.ConfigureAwait(false);

    /// <summary>
    /// Synchronously blocks until the specified <see cref="ValueTask{TResult}"/> completes
    /// and returns its result.
    /// </summary>
    /// <typeparam name="T">The result type of the <see cref="ValueTask{TResult}"/>.</typeparam>
    /// <param name="valueTask">The <see cref="ValueTask{TResult}"/> to wait on.</param>
    /// <returns>The result of the completed operation.</returns>
    /// <remarks>
    /// This method will synchronously block the calling thread and may cause deadlocks
    /// if invoked on a thread with a synchronization context (e.g., UI or ASP.NET).
    /// Prefer <see cref="AwaitSyncSafe{T}(ValueTask{T}, CancellationToken)"/> when safety is required.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T AwaitSync<T>(this ValueTask<T> valueTask) => valueTask.GetAwaiter()
                                                                          .GetResult();

    /// <summary>
    /// Synchronously blocks until the specified <see cref="ValueTask"/> completes.
    /// </summary>
    /// <param name="valueTask">The <see cref="ValueTask"/> to wait on.</param>
    /// <remarks>
    /// This method will synchronously block the calling thread and may cause deadlocks
    /// if invoked on a thread with a synchronization context (e.g., UI or ASP.NET).
    /// Prefer <see cref="AwaitSyncSafe(ValueTask, CancellationToken)"/> when safety is required.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AwaitSync(this System.Threading.Tasks.ValueTask valueTask) => valueTask.GetAwaiter()
                                                                                              .GetResult();

    /// <summary>
    /// Synchronously waits for a <see cref="ValueTask"/> to complete while avoiding
    /// synchronization-context deadlocks.
    /// </summary>
    /// <param name="valueTask">The <see cref="ValueTask"/> to wait on.</param>
    /// <param name="cancellationToken">
    /// An optional <see cref="CancellationToken"/> used to cancel the wait operation.
    /// </param>
    /// <exception cref="OperationCanceledException">
    /// Thrown if the provided <paramref name="cancellationToken"/> is canceled.
    /// </exception>
    /// <remarks>
    /// This method avoids <c>Task.Run</c> and async lambdas by registering a continuation
    /// directly on the <see cref="ValueTask"/> awaiter and blocking until completion.
    /// This significantly reduces allocations while remaining safe for sync-context environments.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AwaitSyncSafe(this System.Threading.Tasks.ValueTask valueTask, CancellationToken cancellationToken = default)
    {
        if (valueTask.IsCompleted)
        {
            valueTask.GetAwaiter()
                     .GetResult();
            return;
        }

        var state = new SyncWaitState(valueTask.GetAwaiter());
        state.Wait(cancellationToken);
        state.RethrowIfFaulted();
    }

    /// <summary>
    /// Synchronously waits for a <see cref="ValueTask{TResult}"/> to complete while avoiding
    /// synchronization-context deadlocks and returns its result.
    /// </summary>
    /// <typeparam name="T">The result type of the <see cref="ValueTask{TResult}"/>.</typeparam>
    /// <param name="valueTask">The <see cref="ValueTask{TResult}"/> to wait on.</param>
    /// <param name="cancellationToken">
    /// An optional <see cref="CancellationToken"/> used to cancel the wait operation.
    /// </param>
    /// <returns>The result of the completed operation.</returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown if the provided <paramref name="cancellationToken"/> is canceled.
    /// </exception>
    /// <remarks>
    /// This method is intended for bridging asynchronous code into synchronous entry points
    /// (e.g., constructors or legacy APIs) while minimizing allocation overhead and avoiding deadlocks.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T AwaitSyncSafe<T>(this ValueTask<T> valueTask, CancellationToken cancellationToken = default)
    {
        if (valueTask.IsCompleted)
            return valueTask.GetAwaiter()
                            .GetResult();

        var state = new SyncWaitState<T>(valueTask.GetAwaiter());
        state.Wait(cancellationToken);
        return state.GetResultOrThrow();
    }

    /// <summary>
    /// Executes the specified <see cref="ValueTask"/> in a fire-and-forget manner,
    /// optionally invoking a callback if an exception occurs.
    /// </summary>
    /// <param name="valueTask">The <see cref="ValueTask"/> to execute.</param>
    /// <param name="onException">
    /// An optional callback invoked if the task faults or is canceled.
    /// If <c>null</c>, exceptions are silently ignored.
    /// </param>
    /// <remarks>
    /// This method ensures that exceptions are always observed to prevent unobserved-task exceptions.
    /// For incomplete tasks, a continuation is registered directly on the awaiter to avoid
    /// async state machine allocations.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FireAndForgetSafe(this System.Threading.Tasks.ValueTask valueTask, Action<Exception>? onException = null)
    {
        if (valueTask.IsCompletedSuccessfully)
            return;

        ValueTaskAwaiter awaiter = valueTask.GetAwaiter();

        if (awaiter.IsCompleted)
        {
            try
            {
                awaiter.GetResult();
            }
            catch (Exception ex)
            {
                onException?.Invoke(ex);
            }

            return;
        }

        var state = new ObserveState(awaiter, onException);
        awaiter.UnsafeOnCompleted(state.Continue);
    }

    /// <summary>
    /// Executes the specified <see cref="ValueTask{TResult}"/> in a fire-and-forget manner,
    /// optionally invoking a callback if an exception occurs.
    /// </summary>
    /// <typeparam name="T">The result type of the <see cref="ValueTask{TResult}"/>.</typeparam>
    /// <param name="valueTask">The <see cref="ValueTask{TResult}"/> to execute.</param>
    /// <param name="onException">
    /// An optional callback invoked if the task faults or is canceled.
    /// If <c>null</c>, exceptions are silently ignored.
    /// </param>
    /// <remarks>
    /// The task result is always consumed to ensure proper completion semantics.
    /// Incomplete tasks are observed via a continuation attached directly to the awaiter
    /// to minimize allocations.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FireAndForgetSafe<T>(this ValueTask<T> valueTask, Action<Exception>? onException = null)
    {
        if (valueTask.IsCompletedSuccessfully)
        {
            _ = valueTask.GetAwaiter()
                         .GetResult();
            return;
        }

        ValueTaskAwaiter<T> awaiter = valueTask.GetAwaiter();

        if (awaiter.IsCompleted)
        {
            try
            {
                _ = awaiter.GetResult();
            }
            catch (Exception ex)
            {
                onException?.Invoke(ex);
            }

            return;
        }

        var state = new ObserveState<T>(awaiter, onException);
        awaiter.UnsafeOnCompleted(state.Continue);
    }
}