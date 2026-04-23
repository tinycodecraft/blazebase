using System;
using System.Runtime.CompilerServices;

namespace Soenneker.Enumerables.CommaSeparated;

/// <summary>
/// Stack-only enumerator that yields trimmed comma-separated tokens without allocations.
/// </summary>
public ref struct CommaSeparatedEnumerator
{
    private ReadOnlySpan<char> _remaining;
    private ReadOnlySpan<char> _current;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CommaSeparatedEnumerator(ReadOnlySpan<char> value)
    {
        // Trim once up-front to match enumerable semantics
        _remaining = value.Trim();
        _current = default;
    }

    /// <summary>
    /// The current token (trimmed, non-empty).
    /// </summary>
    public ReadOnlySpan<char> Current
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _current;
    }

    /// <summary>
    /// Advances to the next token.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MoveNext()
    {
        while (true)
        {
            if (_remaining.IsEmpty)
                return false;

            int idx = _remaining.IndexOf(',');

            if (idx < 0)
            {
                _current = _remaining.Trim();
                _remaining = default;
                return !_current.IsEmpty;
            }

            _current = _remaining[..idx]
                .Trim();
            _remaining = _remaining[(idx + 1)..];

            if (_current.IsEmpty)
                continue; // Skip empty tokens (",," or ", ,")

            return true;
        }
    }
}