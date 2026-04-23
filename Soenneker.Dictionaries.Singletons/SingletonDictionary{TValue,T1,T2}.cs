using System;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Dictionaries.SingletonKeys;
using Soenneker.Dictionaries.Singletons.Abstract;

namespace Soenneker.Dictionaries.Singletons;

/// <summary>
/// A string-keyed singleton dictionary supporting initialization arguments of type <typeparamref name="T1"/> and <typeparamref name="T2"/>.
/// </summary>
public sealed class SingletonDictionary<TValue, T1, T2> : SingletonKeyDictionary<string, TValue, T1, T2>, ISingletonDictionary<TValue, T1, T2>
{
    public SingletonDictionary()
    {
    }

    public SingletonDictionary(Func<string, T1, T2, ValueTask<TValue>> func) : base(func)
    {
    }

    public SingletonDictionary(Func<string, T1, T2, CancellationToken, ValueTask<TValue>> func) : base(func)
    {
    }

    public SingletonDictionary(Func<T1, T2, ValueTask<TValue>> func) : base(func)
    {
    }

    public SingletonDictionary(Func<string, T1, T2, TValue> func) : base(func)
    {
    }

    public SingletonDictionary(Func<string, T1, T2, CancellationToken, TValue> func) : base(func)
    {
    }

    public SingletonDictionary(Func<T1, T2, TValue> func) : base(func)
    {
    }
}
