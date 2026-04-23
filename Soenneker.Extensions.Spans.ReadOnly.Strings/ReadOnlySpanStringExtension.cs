using Soenneker.Utils.PooledStringBuilders;
using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Soenneker.Extensions.Spans.ReadOnly.Strings;

/// <summary>
/// A collection of helpful ReadOnlySpan (string) extension methods
/// </summary>
public static class ReadOnlySpanStringExtension
{
    /// <summary>
    /// Determines whether any element in the specified span contains the given substring, using the specified string
    /// comparison option.
    /// </summary>
    /// <remarks>Null elements within the span are ignored during the search. The search uses the specified
    /// StringComparison value for each element.</remarks>
    /// <param name="span">The read-only span of strings to search.</param>
    /// <param name="part">The substring to seek within each element of the span. Cannot be null.</param>
    /// <param name="comparison">One of the enumeration values that specifies the rules for the substring search, such as case sensitivity and
    /// culture.</param>
    /// <returns>true if any non-null element in the span contains the specified substring; otherwise, false.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAPart(ReadOnlySpan<string> span, string part, StringComparison comparison)
    {
        for (int i = 0; i < span.Length; i++)
        {
            string? current = span[i];

            if (current is not null && current.IndexOf(part, comparison) >= 0)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Concatenates the elements of a read-only span of strings, using the specified separator character between each
    /// element. Optionally inserts a space after each separator.
    /// </summary>
    /// <remarks>Null elements in the span are ignored and do not contribute separators or spaces to the
    /// result. No separator or space is added before the first element or after the last element.</remarks>
    /// <param name="span">A read-only span of strings to join. Null elements are skipped and not included in the result.</param>
    /// <param name="separator">The character to use as a separator between each string element.</param>
    /// <param name="includeSpace">true to insert a space character after each separator; otherwise, false.</param>
    /// <returns>A string consisting of the non-null elements of the span separated by the specified character, with optional
    /// spaces after each separator. Returns an empty string if the span is empty or contains only null elements.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string JoinStrings(ReadOnlySpan<string?> span, char separator, bool includeSpace)
    {
        if (span.Length == 0)
            return string.Empty;

        int initialCapacity = Math.Min(Math.Max(128, span.Length * 4), 4096);
        using var psb = new PooledStringBuilder(initialCapacity);

        string? first = span[0];

        if (first is not null)
            psb.Append(first);

        for (int i = 1; i < span.Length; i++)
        {
            if (includeSpace)
                psb.Append(separator, ' ');
            else
                psb.Append(separator);

            string? s = span[i];

            if (s is not null)
                psb.Append(s);
        }

        return psb.ToString();
    }
}
