using Soenneker.Dictionaries.SingletonKeys.Abstract;

namespace Soenneker.Dictionaries.Singletons.Abstract;

/// <summary>
/// A string-keyed externally initializing singleton dictionary that uses double-check async locking,
/// supporting an initialization argument of type <typeparamref name="T1"/>.
/// </summary>
/// <typeparam name="TValue">The cached value type.</typeparam>
/// <typeparam name="T1">The initialization argument type.</typeparam>
/// <remarks>
/// This is a convenience specialization over <see cref="ISingletonKeyDictionary{TKey,TValue,T1}"/> using <see cref="string"/> keys.
/// </remarks>
public interface ISingletonDictionary<TValue, T1> : ISingletonKeyDictionary<string, TValue, T1>;