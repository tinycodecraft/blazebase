using System;
using System.Runtime.CompilerServices;

namespace Soenneker.Enumerables.CommaSeparated;

/// <summary>
/// Allocation-free enumeration of comma-separated values using <see cref="ReadOnlySpan{char}"/>.
/// Tokens are trimmed and empty entries are skipped.
/// Intended for lightweight CSV-style inputs (no quoting/escaping).
/// </summary>
public readonly ref struct CommaSeparatedEnumerable
{
    private readonly ReadOnlySpan<char> _value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CommaSeparatedEnumerable(ReadOnlySpan<char> value) => _value = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CommaSeparatedEnumerable(string? value) => _value = value.AsSpan();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CommaSeparatedEnumerator GetEnumerator() => new(_value);

    /// <summary>
    /// Counts non-empty, trimmed comma-separated tokens without allocations.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public int Count()
    {
        var count = 0;
        ReadOnlySpan<char> remaining = _value;

        // Optional: trim once or per-token; this matches your enumerator behavior.
        remaining = remaining.Trim();

        while (!remaining.IsEmpty)
        {
            int idx = remaining.IndexOf(',');
            ReadOnlySpan<char> token;

            if (idx < 0)
            {
                token = remaining.Trim();
                remaining = default;
            }
            else
            {
                token = remaining[..idx].Trim();
                remaining = remaining[(idx + 1)..];
            }

            if (!token.IsEmpty)
                count++;
        }

        return count;
    }
}