using Soenneker.Extensions.String;
using Soenneker.Utils.PooledStringBuilders;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Soenneker.Extensions.Spans.ReadOnly.Strings;

namespace Soenneker.Extensions.Enumerable.String;

/// <summary>
/// A collection of helpful enumerable string extension methods.
/// </summary>
public static class EnumerableStringExtension
{
    private const StringComparison _ord = StringComparison.Ordinal;
    private const StringComparison _ordIgnore = StringComparison.OrdinalIgnoreCase;

    private static readonly StringComparer _ordinalIgnoreCase = StringComparer.OrdinalIgnoreCase;

    /// <summary>
    /// Converts each id into a (PartitionKey, DocumentId) tuple via <see cref="StringExtension.ToSplitId(string)"/>.
    /// </summary>
    [Pure]
    public static List<(string PartitionKey, string DocumentId)> ToSplitIds(this IEnumerable<string> ids)
    {
        ArgumentNullException.ThrowIfNull(ids);

        // Fast paths
        if (ids is string[] arr)
        {
            var result = new List<(string PartitionKey, string DocumentId)>(arr.Length);

            for (int i = 0; i < arr.Length; i++)
                result.Add(arr[i].ToSplitId());

            return result;
        }

        if (ids is List<string> list)
        {
            var result = new List<(string PartitionKey, string DocumentId)>(list.Count);
            Span<string> span = CollectionsMarshal.AsSpan(list);

            for (int i = 0; i < span.Length; i++)
                result.Add(span[i].ToSplitId());

            return result;
        }

        // Single fallback (no duplicated foreach blocks)
        int capacity = (ids.TryGetNonEnumeratedCount(out int count) && count > 0) ? count : 0;
        var fallback = capacity > 0 ? new List<(string PartitionKey, string DocumentId)>(capacity) : [];

        foreach (string id in ids)
            fallback.Add(id.ToSplitId());

        return fallback;
    }

    /// <summary>
    /// Returns <c>true</c> if any element contains <paramref name="part"/> using ordinal comparisons.
    /// </summary>
    [Pure]
    public static bool ContainsAPart(this IEnumerable<string>? enumerable, string part, bool ignoreCase = true)
    {
        if (enumerable is null || part.IsNullOrEmpty())
            return false;

        if (enumerable is ICollection<string> { Count: 0 })
            return false;

        StringComparison comparison = ignoreCase ? _ordIgnore : _ord;

        // Collapse the branching: same perf characteristics, less duplication.
        if (enumerable is string[] arr)
            return ReadOnlySpanStringExtension.ContainsAPart(arr, part, comparison);

        if (enumerable is List<string> list)
            return ReadOnlySpanStringExtension.ContainsAPart(CollectionsMarshal.AsSpan(list), part, comparison);

        if (enumerable is IList<string> ilist)
        {
            for (int i = 0; i < ilist.Count; i++)
            {
                string? current = ilist[i];
                if (current is not null && current.IndexOf(part, comparison) >= 0)
                    return true;
            }

            return false;
        }

        foreach (string? current in enumerable)
        {
            if (current is not null && current.IndexOf(part, comparison) >= 0)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Equivalent to <see cref="ToSeparatedString{T}(IEnumerable{T}?, char, bool)"/> with a comma separator.
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToCommaSeparatedString<T>(
        this IEnumerable<T>? enumerable,
        bool includeSpace = false)
    {
        if (enumerable is null)
            return string.Empty;

        // Fast-path: ICollection<T> -> let string.Join do the heavy lifting (usually fastest)
        if (enumerable is ICollection<T> collection)
        {
            int count = collection.Count;

            if (count == 0)
                return string.Empty;

            if (count == 1)
            {
                using var e = collection.GetEnumerator();
                e.MoveNext();
                return e.Current?.ToString() ?? string.Empty;
            }

            return string.Join(includeSpace ? ", " : ",", collection);
        }

        // Fast-path: IReadOnlyCollection<T> (count only)
        if (enumerable is IReadOnlyCollection<T> readOnly)
        {
            int count = readOnly.Count;

            if (count == 0)
                return string.Empty;

            if (count == 1)
            {
                using var e = enumerable.GetEnumerator();
                e.MoveNext();
                return e.Current?.ToString() ?? string.Empty;
            }
        }

        // Fallback: unknown enumerable -> pooled builder
        using IEnumerator<T> enumerator = enumerable.GetEnumerator();

        if (!enumerator.MoveNext())
            return string.Empty;

        T first = enumerator.Current;

        if (!enumerator.MoveNext())
            return first?.ToString() ?? string.Empty;

        using var psb = new PooledStringBuilder();

        psb.Append(first as string ?? first?.ToString());

        // We already know there's at least one more item in enumerator.Current right now
        while (true)
        {
            psb.Append(',');

            if (includeSpace)
                psb.Append(' ');

            // enumerator.Current is already the "next" item at loop start
            var current = enumerator.Current;
            psb.Append(current as string ?? current?.ToString());

            if (!enumerator.MoveNext())
                break;
        }

        return psb.ToString();
    }

    /// <summary>
    /// Fast-path join for <typeparamref name="T"/> that implements <see cref="ISpanFormattable"/>.
    /// Avoids boxing that can occur in the unconstrained join overload.
    /// </summary>
    [Pure]
    public static string ToSeparatedStringFormattable<T>(this IEnumerable<T>? enumerable, char separator, bool includeSpace = false)
        where T : ISpanFormattable
    {
        if (enumerable is null)
            return string.Empty;

        if (enumerable is ICollection<T> { Count: 0 })
            return string.Empty;

        if (enumerable is T[] arr)
            return JoinFormattables(arr, separator, includeSpace);

        if (enumerable is List<T> list)
            return JoinFormattables(CollectionsMarshal.AsSpan(list), separator, includeSpace);

        int initialCapacity = GetInitialJoinCapacity(enumerable);
        var psb = new PooledStringBuilder(initialCapacity);
        bool disposed = false;

        try
        {
            bool wroteAny = false;

            foreach (T item in enumerable)
            {
                if (wroteAny)
                {
                    if (includeSpace)
                        psb.Append(separator, ' ');
                    else
                        psb.Append(separator);
                }
                else
                {
                    wroteAny = true;
                }

                psb.Append(item);
            }

            if (!wroteAny)
            {
                psb.Dispose();
                disposed = true;
                return string.Empty;
            }

            disposed = true;
            return psb.ToStringAndDispose();
        }
        finally
        {
            if (!disposed)
                psb.Dispose();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string JoinFormattables<T>(ReadOnlySpan<T> span, char separator, bool includeSpace)
        where T : ISpanFormattable
    {
        if (span.Length == 0)
            return string.Empty;

        int initialCapacity = Math.Min(Math.Max(128, span.Length * 4), 4096);
        using var psb = new PooledStringBuilder(initialCapacity);

        psb.Append(span[0]);

        for (int i = 1; i < span.Length; i++)
        {
            if (includeSpace)
                psb.Append(separator, ' ');
            else
                psb.Append(separator);

            psb.Append(span[i]);
        }

        return psb.ToString();

    }

    /// <summary>
    /// Joins the elements into a single string using <paramref name="separator"/> (optionally followed by a space).
    /// Null items match <see cref="string.Join(string?, IEnumerable{string?})"/> semantics (treated as empty).
    /// </summary>
    [Pure]
    public static string ToSeparatedString<T>(this IEnumerable<T>? enumerable, char separator, bool includeSpace = false)
    {
        if (enumerable is null)
            return string.Empty;

        if (enumerable is ICollection<T> { Count: 0 })
            return string.Empty;

        // Tight string fast-paths
        if (typeof(T) == typeof(string))
        {
            // Only valid when no added-space; BCL join is extremely optimized.
            if (!includeSpace && enumerable is IEnumerable<string?> s1)
                return string.Join(separator, s1);

            if (enumerable is string?[] arr)
                return ReadOnlySpanStringExtension.JoinStrings(arr, separator, includeSpace);

            if (enumerable is List<string?> list)
                return ReadOnlySpanStringExtension.JoinStrings(CollectionsMarshal.AsSpan(list), separator, includeSpace);
        }

        int initialCapacity = GetInitialJoinCapacity(enumerable);
        var psb = new PooledStringBuilder(initialCapacity);
        bool disposed = false;

        try
        {
            bool wroteAny = false;

            foreach (T item in enumerable)
            {
                if (wroteAny)
                {
                    if (includeSpace)
                        psb.Append(separator, ' ');
                    else
                        psb.Append(separator);
                }
                else
                {
                    wroteAny = true;
                }

                if (item is null)
                    continue;

                if (item is string s)
                {
                    psb.Append(s);
                    continue;
                }

                // Note: for value-types, "is ISpanFormattable" boxes.
                if (item is ISpanFormattable sf)
                {
                    AppendFormattable(ref psb, sf);
                    continue;
                }

                psb.Append(item.ToString());
            }

            if (!wroteAny)
            {
                psb.Dispose();
                disposed = true;
                return string.Empty;
            }

            disposed = true;
            return psb.ToStringAndDispose();
        }
        finally
        {
            if (!disposed)
                psb.Dispose();
        }
    }

    [Pure]
    public static IEnumerable<string> ToLower(this IEnumerable<string> enumerable)
    {
        ArgumentNullException.ThrowIfNull(enumerable);

        foreach (string? str in enumerable)
            yield return str?.ToLowerInvariantFast() ?? "";
    }

    [Pure]
    public static IEnumerable<string> ToUpper(this IEnumerable<string> enumerable)
    {
        ArgumentNullException.ThrowIfNull(enumerable);

        foreach (string? str in enumerable)
            yield return str?.ToUpperInvariantFast() ?? "";
    }

    [Pure]
    public static HashSet<string> ToHashSetIgnoreCase(this IEnumerable<string> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source is HashSet<string> hs && ReferenceEquals(hs.Comparer, _ordinalIgnoreCase))
            return new HashSet<string>(hs, _ordinalIgnoreCase);

        HashSet<string> hashSet =
            source.TryGetNonEnumeratedCount(out int count) && count > 0
                ? new HashSet<string>(count, _ordinalIgnoreCase)
                : new HashSet<string>(_ordinalIgnoreCase);

        if (source is string[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
                hashSet.Add(arr[i]);

            return hashSet;
        }

        if (source is List<string> list)
        {
            Span<string> span = CollectionsMarshal.AsSpan(list);
            for (int i = 0; i < span.Length; i++)
                hashSet.Add(span[i]);

            return hashSet;
        }

        if (source is IList<string> ilist)
        {
            for (int i = 0; i < ilist.Count; i++)
                hashSet.Add(ilist[i]);

            return hashSet;
        }

        foreach (string item in source)
            hashSet.Add(item);

        return hashSet;
    }

    [Pure]
    public static IEnumerable<string> RemoveNullOrEmpty(this IEnumerable<string> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        foreach (string? str in source)
        {
            if (str.HasContent())
                yield return str!;
        }
    }

    [Pure]
    public static IEnumerable<string> RemoveNullOrWhiteSpace(this IEnumerable<string> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        foreach (string? str in source)
        {
            if (!string.IsNullOrWhiteSpace(str))
                yield return str!;
        }
    }

    [Pure]
    public static IEnumerable<string> DistinctIgnoreCase(this IEnumerable<string> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        HashSet<string> seen =
            source.TryGetNonEnumeratedCount(out int count) && count > 0
                ? new HashSet<string>(count, _ordinalIgnoreCase)
                : new HashSet<string>(_ordinalIgnoreCase);

        if (source is string[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                string? str = arr[i];
                if (str is not null && seen.Add(str))
                    yield return str;
            }

            yield break;
        }

        // IMPORTANT: iterator method -> no Span<T> locals allowed here
        if (source is List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                string? str = list[i];
                if (str is not null && seen.Add(str))
                    yield return str;
            }

            yield break;
        }

        if (source is IList<string> ilist)
        {
            for (int i = 0; i < ilist.Count; i++)
            {
                string? str = ilist[i];
                if (str is not null && seen.Add(str))
                    yield return str;
            }

            yield break;
        }

        foreach (string? str in source)
        {
            if (str is not null && seen.Add(str))
                yield return str;
        }
    }

    [Pure]
    public static bool StartsWithIgnoreCase(this IEnumerable<string> source, string prefix)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(prefix);

        if (prefix.Length == 0)
        {
            foreach (string? s in source)
                if (s is not null)
                    return true;

            return false;
        }

        if (source is string[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                string? str = arr[i];
                if (str is not null && str.StartsWith(prefix, _ordIgnore))
                    return true;
            }

            return false;
        }

        if (source is List<string> list)
        {
            Span<string> span = CollectionsMarshal.AsSpan(list);

            for (int i = 0; i < span.Length; i++)
            {
                string? str = span[i];
                if (str is not null && str.StartsWith(prefix, _ordIgnore))
                    return true;
            }

            return false;
        }

        foreach (string? str in source)
        {
            if (str is not null && str.StartsWith(prefix, _ordIgnore))
                return true;
        }

        return false;
    }

    [Pure]
    public static bool EndsWithIgnoreCase(this IEnumerable<string> source, string suffix)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(suffix);

        if (suffix.Length == 0)
        {
            foreach (string? s in source)
                if (s is not null)
                    return true;

            return false;
        }

        if (source is string[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                string? str = arr[i];
                if (str is not null && str.EndsWith(suffix, _ordIgnore))
                    return true;
            }

            return false;
        }

        if (source is List<string> list)
        {
            Span<string> span = CollectionsMarshal.AsSpan(list);

            for (int i = 0; i < span.Length; i++)
            {
                string? str = span[i];
                if (str is not null && str.EndsWith(suffix, _ordIgnore))
                    return true;
            }

            return false;
        }

        foreach (string? str in source)
        {
            if (str is not null && str.EndsWith(suffix, _ordIgnore))
                return true;
        }

        return false;
    }

    [Pure]
    public static bool ContainsIgnoreCase(this IEnumerable<string> source, string value)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(value);

        if (source is HashSet<string> hs && ReferenceEquals(hs.Comparer, _ordinalIgnoreCase))
            return hs.Contains(value);

        if (source is string[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                string? str = arr[i];
                if (str is not null && _ordinalIgnoreCase.Equals(str, value))
                    return true;
            }

            return false;
        }

        if (source is List<string> list)
        {
            Span<string> span = CollectionsMarshal.AsSpan(list);

            for (int i = 0; i < span.Length; i++)
            {
                string? str = span[i];
                if (str is not null && _ordinalIgnoreCase.Equals(str, value))
                    return true;
            }

            return false;
        }

        if (source is IList<string> ilist)
        {
            for (int i = 0; i < ilist.Count; i++)
            {
                string? str = ilist[i];
                if (str is not null && _ordinalIgnoreCase.Equals(str, value))
                    return true;
            }

            return false;
        }

        foreach (string? str in source)
        {
            if (str is not null && _ordinalIgnoreCase.Equals(str, value))
                return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetInitialJoinCapacity<T>(IEnumerable<T> enumerable)
    {
        if (enumerable.TryGetNonEnumeratedCount(out int count) && count > 0)
            return Math.Min(Math.Max(128, count * 4), 4096);

        return 128;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendFormattable(
        ref PooledStringBuilder psb,
        ISpanFormattable value,
        ReadOnlySpan<char> format = default,
        IFormatProvider? provider = null)
    {
        int hint = 32;

        while (true)
        {
            Span<char> span = psb.AppendSpan(hint);

            if (value.TryFormat(span, out int written, format, provider))
            {
                psb.Shrink(hint - written);
                return;
            }

            psb.Shrink(hint);

            if (hint >= (1 << 20))
            {
                psb.Append(value.ToString());
                return;
            }

            hint <<= 1;
        }
    }
}
