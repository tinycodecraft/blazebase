using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Extensions.Task;

/// <summary>
/// A collection of helpful Task extension methods
/// </summary>
public static class TaskExtension
{
    /// <summary>
    /// Configures an awaiter used to await this <see cref="Task"/> to continue on a different context.
    /// Equivalent to <code>task.ConfigureAwait(false);</code>.
    /// </summary>
    /// <param name="task">The <see cref="Task"/> to configure.</param>
    /// <returns>A configured task awaitable.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConfiguredTaskAwaitable NoSync(this System.Threading.Tasks.Task task)
    {
        return task.ConfigureAwait(false);
    }

    /// <summary>
    /// Configures an awaiter used to await this <see cref="Task{TResult}"/> to continue on a different context.
    /// Equivalent to <code>task.ConfigureAwait(false);</code>.
    /// </summary>
    /// <typeparam name="T">The type of the result produced by this <see cref="Task{TResult}"/>.</typeparam>
    /// <param name="task">The <see cref="Task{TResult}"/> to configure.</param>
    /// <returns>A configured task awaitable.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConfiguredTaskAwaitable<T> NoSync<T>(this Task<T> task)
    {
        return task.ConfigureAwait(false);
    }

    /// <summary>
    /// Converts a <see cref="Task"/> to a <see cref="ValueTask"/>. 
    /// If the task is already completed successfully, returns a completed <see cref="ValueTask"/>. 
    /// Equivalent to <code>new ValueTask(task)</code>.
    /// </summary>
    /// <param name="task">The <see cref="Task"/> to convert.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask ToValueTask(this System.Threading.Tasks.Task task)
    {
        if (task.IsCompletedSuccessfully)
            return default;

        return new ValueTask(task);
    }

    /// <summary>
    /// Converts a <see cref="Task{TResult}"/> to a <see cref="ValueTask{TResult}"/>. 
    /// If the task is already completed successfully, returns a completed <see cref="ValueTask{TResult}"/> with the result.
    /// Equivalent to <code>new ValueTask(task)</code>.
    /// </summary>
    /// <typeparam name="T">The type of the result produced by the task.</typeparam>
    /// <param name="task">The <see cref="Task{TResult}"/> to convert.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the asynchronous operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<T> ToValueTask<T>(this Task<T> task)
    {
        if (task.IsCompletedSuccessfully)
            return new ValueTask<T>(task.GetAwaiter()
                                        .GetResult());

        return new ValueTask<T>(task);
    }

    /// <summary>
    /// Synchronously awaits the specified <see cref="Task"/>.
    /// </summary>
    /// <param name="task">The <see cref="Task"/> to await synchronously.</param>
    /// <remarks>
    /// This method blocks the calling thread until the task completes. This may lead to deadlocks
    /// if called on a context that does not allow synchronous blocking (e.g., UI thread).
    /// </remarks>
    /// <exception cref="OperationCanceledException">The task was canceled.</exception>
    /// <exception cref="Exception">The task faulted and threw an exception.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AwaitSync(this System.Threading.Tasks.Task task)
    {
        task.GetAwaiter()
            .GetResult();
    }

    /// <summary>
    /// Synchronously awaits the specified <see cref="Task{TResult}"/> and returns its result.
    /// </summary>
    /// <typeparam name="T">The result type of the <see cref="Task{T}"/>.</typeparam>
    /// <param name="task">The <see cref="Task{T}"/> to await synchronously.</param>
    /// <returns>The result of the completed <see cref="Task{T}"/>.</returns>
    /// <remarks>
    /// This method blocks the calling thread until the task completes. This may lead to deadlocks
    /// if called on a context that does not allow synchronous blocking (e.g., UI thread).
    /// </remarks>
    /// <exception cref="OperationCanceledException">The task was canceled.</exception>
    /// <exception cref="Exception">The task faulted and threw an exception.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T AwaitSync<T>(this Task<T> task)
    {
        return task.GetAwaiter()
                   .GetResult();
    }

    // These helpers still create an async state machine per call (inevitable if we truly "await"),
    // but we avoid the *extra* closure allocations from async lambdas capturing locals.
    private static async System.Threading.Tasks.Task AwaitTaskCore(System.Threading.Tasks.Task task) => await task.ConfigureAwait(false);

    private static async Task<T> AwaitTaskCoreT<T>(Task<T> task) => await task.ConfigureAwait(false);

    /// <summary>
    /// Attempts to synchronously wait in a way that avoids common deadlocks by running the await on the ThreadPool.
    /// This is still not a silver bullet; prefer async all the way when possible.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AwaitSyncSafe(this System.Threading.Tasks.Task task, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);

        if (task.IsCompleted)
        {
            task.GetAwaiter()
                .GetResult();
            return;
        }

        // Avoid: Task.Run(async () => await task...) (closure + async state machine)
        // Use StartNew with a static delegate + Unwrap to reduce allocations.
        System.Threading.Tasks.Task.Factory.StartNew(static state => AwaitTaskCore((System.Threading.Tasks.Task)state!), task, cancellationToken,
                  TaskCreationOptions.DenyChildAttach, TaskScheduler.Default)
              .Unwrap()
              .GetAwaiter()
              .GetResult();
    }

    /// <summary>
    /// Attempts to synchronously wait in a way that avoids common deadlocks by running the await on the ThreadPool.
    /// This is still not a silver bullet; prefer async all the way when possible.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T AwaitSyncSafe<T>(this Task<T> task, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);

        if (task.IsCompleted)
            return task.GetAwaiter()
                       .GetResult();

        return System.Threading.Tasks.Task.Factory.StartNew(static state => AwaitTaskCoreT((Task<T>)state!), task, cancellationToken,
                         TaskCreationOptions.DenyChildAttach, TaskScheduler.Default)
                     .Unwrap()
                     .GetAwaiter()
                     .GetResult();
    }

    /// <summary>
    /// Fires the task without awaiting it, while ensuring any exceptions are observed.
    /// Optionally forwards the exception to <paramref name="onException"/>.
    /// </summary>
    /// <param name="task">The task to run in a fire-and-forget manner.</param>
    /// <param name="onException">Optional handler invoked if the task faults.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FireAndForgetSafe(this System.Threading.Tasks.Task task, Action<Exception>? onException = null)
    {
        ArgumentNullException.ThrowIfNull(task);

        if (task.IsCompletedSuccessfully)
            return;

        if (onException is null)
        {
            // Observe exception if it faults (continuation still allocates, but avoids base-exception work)
            _ = task.ContinueWith(static t => _ = t.Exception, CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

            return;
        }

        _ = task.ContinueWith(static (t, state) =>
        {
            Exception ex = t.Exception!.GetBaseException();
            ((Action<Exception>)state!).Invoke(ex);
        }, onException, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }


    /// <summary>
    /// Fires the task without awaiting it, while ensuring any exceptions are observed.
    /// Optionally forwards the exception to <paramref name="onException"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FireAndForgetSafe<T>(this Task<T> task, Action<Exception>? onException = null)
    {
        ArgumentNullException.ThrowIfNull(task);

        if (task.IsCompletedSuccessfully)
            return;

        ((System.Threading.Tasks.Task)task).FireAndForgetSafe(onException);
    }
}