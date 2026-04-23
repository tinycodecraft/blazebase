using System;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Dictionaries.SingletonKeys;
using Soenneker.Dictionaries.Singletons.Abstract;

namespace Soenneker.Dictionaries.Singletons;

/// <summary>
/// A string-keyed externally initializing singleton dictionary that uses double-check async locking,
/// with optional async and sync disposal.
/// </summary>
public sealed class SingletonDictionary<TValue> : SingletonKeyDictionary<string, TValue>, ISingletonDictionary<TValue>
{
    public SingletonDictionary()
    {
    }

    public SingletonDictionary(Func<string, ValueTask<TValue>> func) : base(func)
    {
    }

    public SingletonDictionary(Func<string, CancellationToken, ValueTask<TValue>> func) : base(func)
    {
    }

    public SingletonDictionary(Func<ValueTask<TValue>> func) : base(func)
    {
    }

    public SingletonDictionary(Func<string, TValue> func) : base(func)
    {
    }

    public SingletonDictionary(Func<string, CancellationToken, TValue> func) : base(func)
    {
    }

    public SingletonDictionary(Func<TValue> func) : base(func)
    {
    }

    /// <summary>
    /// Fluent typed wrapper around <see cref="SingletonKeyDictionary{TKey,TValue}.Initialize{TState}"/>.
    /// </summary>
    public new SingletonDictionary<TValue> Initialize<TState>(TState state, Func<TState, string, CancellationToken, ValueTask<TValue>> factory)
        where TState : notnull
    {
        base.Initialize(state, factory);
        return this;
    }
}