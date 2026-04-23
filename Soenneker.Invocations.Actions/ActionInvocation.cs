using System;
using System.Runtime.CompilerServices;

namespace Soenneker.Invocations.Actions;

/// <summary>
/// Deferred, stateful synchronous action invocation without closure capture.
/// </summary>
public sealed class ActionInvocation
{
    private readonly Action<object?> _action;

    public object? State { get; }

    public ActionInvocation(Action<object?> action, object? state)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
        State = state;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Invoke() => _action(State);
}