using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using Soenneker.Extensions.Long;
using Soenneker.Extensions.Spans.Chars;

namespace Soenneker.Blazor.Utils.Ids;

/// <summary>
/// A lightweight ID generator for consistent identity across the UI for Blazor components.
/// </summary>
public static class BlazorIdGenerator
{
    private static long _count;

    /// <summary>
    /// Generates a new unique, human-readable ID using the specified prefix.
    /// </summary>
    /// <param name="prefix">
    /// The prefix to prepend to the generated ID. This value must not be null, empty, or whitespace.
    /// </param>
    /// <returns>
    /// A unique ID in the format "{prefix}-{number}", where <c>number</c> is a monotonically increasing value.
    /// </returns>
    /// <remarks>
    /// The generated ID is guaranteed to be unique within the current process and is suitable for use in DOM elements
    /// and ARIA relationships. The numeric portion is generated using a thread-safe incrementing counter.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="prefix"/> is null, empty, or consists only of whitespace.
    /// </exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string New(string prefix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);

        long next = Interlocked.Increment(ref _count);
        int digits = next.DigitCountPositiveOnly();

        return string.Create(prefix.Length + 1 + digits, (prefix, next, digits), static (span, state) =>
        {
            state.prefix.AsSpan()
                 .CopyTo(span);

            int pos = state.prefix.Length;
            span[pos++] = '-';

            span[pos..]
                .WritePositiveInt64(state.next, state.digits);
        });
    }

    /// <summary>
    /// Creates a derived child ID by appending a suffix to an existing parent ID.
    /// </summary>
    /// <param name="parentId">
    /// The base ID to extend. This value must not be null, empty, or whitespace.
    /// </param>
    /// <param name="suffix">
    /// The suffix to append to the parent ID. This value must not be null, empty, or whitespace.
    /// </param>
    /// <returns>
    /// A new ID in the format "{parentId}-{suffix}".
    /// </returns>
    /// <remarks>
    /// This method is typically used to generate related element IDs (e.g., trigger, content, label)
    /// that share a common root identifier for accessibility and structural consistency.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="parentId"/> or <paramref name="suffix"/> is null, empty, or consists only of whitespace.
    /// </exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Child(string parentId, string suffix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(suffix);

        return string.Concat(parentId, "-", suffix);
    }
}