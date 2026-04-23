using System;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Dictionaries.SingletonKeys;
using Soenneker.Dictionaries.Singletons.Abstract;

namespace Soenneker.Dictionaries.Singletons;

/// <summary>
/// A string-keyed singleton dictionary supporting an initialization argument of type <typeparamref name="T1"/>.
/// </summary>
public sealed class SingletonDictionary<TValue, T1> : SingletonKeyDictionary<string, TValue, T1>, ISingletonDictionary<TValue, T1>
{
    public SingletonDictionary()
    {
    }

    public SingletonDictionary(Func<string, T1, ValueTask<TValue>> func) : base(func)
    {
    }

    public SingletonDictionary(Func<string, T1, CancellationToken, ValueTask<TValue>> func) : base(func)
    {
    }

    public SingletonDictionary(Func<T1, ValueTask<TValue>> func) : base(func)
    {
    }

    public SingletonDictionary(Func<string, T1, TValue> func) : base(func)
    {
    }

    public SingletonDictionary(Func<string, T1, CancellationToken, TValue> func) : base(func)
    {
    }

    public SingletonDictionary(Func<T1, TValue> func) : base(func)
    {
    }
}