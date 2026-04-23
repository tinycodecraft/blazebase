using Soenneker.Dictionaries.SingletonKeys.Abstract;

namespace Soenneker.Dictionaries.Singletons.Abstract;

/// <summary>
/// A string-keyed externally initializing singleton dictionary that uses double-check async locking,
/// with optional async and sync disposal.
/// </summary>
/// <typeparam name="TValue">The cached value type.</typeparam>
/// <remarks>
/// This is a convenience specialization over <see cref="ISingletonKeyDictionary{TKey,TValue}"/> using <see cref="string"/> keys.
/// </remarks>
public interface ISingletonDictionary<TValue> : ISingletonKeyDictionary<string, TValue>;