using Soenneker.Utils.Random;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Soenneker.Extensions.List;

/// <summary>
/// A collection of high-performance extension methods for working with lists and list-like collections.
/// </summary>
/// <remarks>
/// These extensions are designed to minimize allocations and bounds checks by leveraging
/// advanced C# techniques where possible and by avoiding LINQ and
/// unnecessary interface dispatch in hot paths.
/// </remarks>
public static class ListExtension
{
    /// <summary>
    /// Replaces the first element in the list that matches the specified predicate with a new value.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="list">The list to search. If null or empty, the method does nothing.</param>
    /// <param name="match">The predicate used to identify the element to replace.</param>
    /// <param name="newItem">The value to assign to the first matching element.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="match"/> is null.</exception>
    /// <remarks>
    /// Only the first matching element is replaced. The operation runs in O(n) time and does not
    /// allocate. The list is modified in place.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Replace<T>(this List<T>? list, Predicate<T> match, T newItem)
    {
        if (list is null || list.Count == 0)
            return;

        if (match is null)
            throw new ArgumentNullException(nameof(match));

        Span<T> span = CollectionsMarshal.AsSpan(list);

        for (int i = 0; i < span.Length; i++)
        {
            if (match(span[i]))
            {
                span[i] = newItem;
                return;
            }
        }
    }

    /// <summary>
    /// Removes the first element in the list that matches the specified predicate.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="list">The list to modify. If null or empty, the method does nothing.</param>
    /// <param name="match">The predicate used to identify the element to remove.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="match"/> is null.</exception>
    /// <remarks>
    /// Only the first matching element is removed. The order of remaining elements is preserved.
    /// This method avoids redundant internal shifting by performing a single manual shift followed
    /// by a tail removal.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Remove<T>(this List<T>? list, Predicate<T> match)
    {
        if (list is null || list.Count == 0)
            return;

        if (match is null)
            throw new ArgumentNullException(nameof(match));

        Span<T> span = CollectionsMarshal.AsSpan(list);

        for (int i = 0; i < span.Length; i++)
        {
            if (match(span[i]))
            {
                int last = span.Length - 1;

                if (i < last)
                    span.Slice(i + 1, last - i).CopyTo(span.Slice(i));

                list.RemoveAt(last);
                return;
            }
        }
    }

    /// <summary>
    /// Randomly reorders the elements of the list in place using a fast, non-cryptographic RNG.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="list">
    /// The list to shuffle. If null or containing fewer than two elements, the method does nothing.
    /// </param>
    /// <remarks>
    /// Implements the Fisher–Yates shuffle algorithm. For <see cref="List{T}"/>, the implementation
    /// uses span-based access to minimize bounds checks and interface dispatch.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Shuffle<T>(this IList<T>? list)
    {
        if (list is null)
            return;

        int n = list.Count;
        if (n < 2)
            return;

        if (list is List<T> l)
        {
            ShuffleList(l);
            return;
        }

        while (n > 1)
        {
            int k = RandomUtil.Next(n);
            n--;

            T tmp = list[n];
            list[n] = list[k];
            list[k] = tmp;
        }
    }

    /// <summary>
    /// Randomly reorders the elements of a <see cref="List{T}"/> using a span-optimized shuffle.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ShuffleList<T>(List<T> list)
    {
        Span<T> span = CollectionsMarshal.AsSpan(list);

        int n = span.Length;
        while (n > 1)
        {
            int k = RandomUtil.Next(n);
            n--;

            T tmp = span[n];
            span[n] = span[k];
            span[k] = tmp;
        }
    }

    /// <summary>
    /// Randomly reorders the elements of the list in place using a cryptographically secure RNG.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="list">
    /// The list to shuffle. If null or containing fewer than two elements, the method does nothing.
    /// </param>
    /// <remarks>
    /// This method uses <see cref="RandomNumberGenerator.GetInt32(int)"/> and is suitable for
    /// security-sensitive scenarios. It is slower than <see cref="Shuffle{T}(IList{T}?)"/>.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SecureShuffle<T>(this IList<T>? list)
    {
        if (list is null)
            return;

        int n = list.Count;
        if (n < 2)
            return;

        if (list is List<T> l)
        {
            SecureShuffleList(l);
            return;
        }

        while (n > 1)
        {
            int k = RandomNumberGenerator.GetInt32(n);
            n--;

            T tmp = list[n];
            list[n] = list[k];
            list[k] = tmp;
        }
    }

    /// <summary>
    /// Cryptographically secure shuffle optimized for <see cref="List{T}"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SecureShuffleList<T>(List<T> list)
    {
        Span<T> span = CollectionsMarshal.AsSpan(list);

        int n = span.Length;
        while (n > 1)
        {
            int k = RandomNumberGenerator.GetInt32(n);
            n--;

            T tmp = span[n];
            span[n] = span[k];
            span[k] = tmp;
        }
    }

    /// <summary>
    /// Returns a random element from the list, or the default value if the list is null or empty.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="list">The list to sample from.</param>
    /// <returns>
    /// A randomly selected element, or <c>default</c> if the list is null or contains no elements.
    /// </returns>
    /// <remarks>
    /// Runs in O(1) time. If the list contains exactly one element, that element is returned
    /// without invoking the RNG.
    /// </remarks>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? GetRandom<T>(this IList<T>? list)
    {
        if (list is null)
            return default;

        int count = list.Count;
        if (count == 0)
            return default;

        return count == 1 ? list[0] : list[RandomUtil.Next(count)];
    }

    /// <summary>
    /// Creates a new <see cref="HashSet{T}"/> containing the elements of the list.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="list">The source list. May be null.</param>
    /// <param name="comparer">
    /// The equality comparer to use, or null to use the default comparer.
    /// </param>
    /// <returns>
    /// A new <see cref="HashSet{T}"/> containing the elements of the list, or an empty set if
    /// the list is null or empty.
    /// </returns>
    /// <remarks>
    /// The set is pre-sized to the list count to minimize rehashing. When the source is a
    /// <see cref="List{T}"/>, span-based enumeration is used for optimal performance.
    /// </remarks>
    [Pure]
    public static HashSet<T> ToHashSet<T>(this IList<T>? list, IEqualityComparer<T>? comparer = null)
    {
        if (list is null)
            return comparer is null ? new HashSet<T>() : new HashSet<T>(comparer);

        int count = list.Count;
        if (count == 0)
            return comparer is null ? new HashSet<T>() : new HashSet<T>(comparer);

        var set = comparer is null ? new HashSet<T>(count) : new HashSet<T>(count, comparer);

        if (list is List<T> l)
        {
            Span<T> span = CollectionsMarshal.AsSpan(l);
            for (int i = 0; i < span.Length; i++)
                set.Add(span[i]);

            return set;
        }

        for (int i = 0; i < count; i++)
            set.Add(list[i]);

        return set;
    }

    /// <summary>
    /// Removes all elements from the list that match the specified predicate and state object.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="list">The list to modify.</param>
    /// <param name="match">
    /// A predicate that returns true for elements that should be removed.
    /// </param>
    /// <param name="state">
    /// Optional state passed to the predicate to support context-aware matching.
    /// </param>
    /// <returns>The number of elements removed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="match"/> is null.</exception>
    /// <remarks>
    /// Preserves element order and runs in O(n) time. This implementation avoids repeated
    /// shifting by compacting surviving elements in a single pass before trimming the tail.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RemoveAll<T>(this List<T> list, Func<T, object?, bool> match, object? state)
    {
        if (list is null || list.Count == 0)
            return 0;

        if (match is null)
            throw new ArgumentNullException(nameof(match));

        Span<T> span = CollectionsMarshal.AsSpan(list);
        int count = span.Length;

        int freeIndex = 0;
        while ((uint)freeIndex < (uint)count && !match(span[freeIndex], state))
            freeIndex++;

        if (freeIndex >= count)
            return 0;

        int current = freeIndex + 1;

        while (current < count)
        {
            while (current < count && match(span[current], state))
                current++;

            if (current < count)
                span[freeIndex++] = span[current++];
        }

        int removed = count - freeIndex;
        list.RemoveRange(freeIndex, removed);
        return removed;
    }
}
