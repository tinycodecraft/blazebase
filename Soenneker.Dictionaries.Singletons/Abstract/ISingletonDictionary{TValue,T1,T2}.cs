using Soenneker.Dictionaries.SingletonKeys.Abstract;

namespace Soenneker.Dictionaries.Singletons.Abstract;

/// <summary>
/// A string-keyed externally initializing singleton dictionary that uses double-check async locking,
/// supporting initialization arguments of type <typeparamref name="T1"/> and <typeparamref name="T2"/>.
/// </summary>
/// <typeparam name="TValue">The cached value type.</typeparam>
/// <typeparam name="T1">The first initialization argument type.</typeparam>
/// <typeparam name="T2">The second initialization argument type.</typeparam>
/// <remarks>
/// This is a convenience specialization over <see cref="ISingletonKeyDictionary{TKey,TValue,T1,T2}"/> using <see cref="string"/> keys.
/// </remarks>
public interface ISingletonDictionary<TValue, T1, T2> : ISingletonKeyDictionary<string, TValue, T1, T2>;


