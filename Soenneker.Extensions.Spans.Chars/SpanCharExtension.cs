using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Soenneker.Extensions.Spans.Bytes;

namespace Soenneker.Extensions.Spans.Chars;

/// <summary>
/// Various helpful character span extension methods.
/// </summary>
public static class SpanCharExtension
{
    /// <summary>
    /// Overwrites the contents of the specified character span with zeros in a manner designed to prevent sensitive
    /// data from lingering in memory.
    /// </summary>
    /// <remarks>
    /// This method is intended for use when handling sensitive information, such as passwords or cryptographic keys,
    /// to help reduce the risk of data remaining in memory after it is no longer needed.
    /// The operation is performed in a way that is intended to prevent the compiler or runtime from optimizing away the memory clearing.
    /// </remarks>
    /// <param name="span">The span of characters to be securely cleared.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SecureZero(this Span<char> span)
    {
        MemoryMarshal.AsBytes(span).SecureZero();
    }

    /// <summary>
    /// Writes the decimal representation of the specified <see cref="long"/> value into the provided <see cref="Span{Char}"/>.
    /// </summary>
    /// <param name="destination">
    /// The destination span that will receive the characters. Its length must be greater than or equal to <paramref name="digits"/>.
    /// </param>
    /// <param name="value">The value to write.</param>
    /// <param name="digits">
    /// The exact number of characters required to represent <paramref name="value"/>, including the minus sign for negative values.
    /// </param>
    /// <remarks>
    /// This method does not perform bounds checking or validation for performance reasons.
    /// Passing an incorrect <paramref name="digits"/> value or a destination span that is too small will result in incorrect behavior.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteInt64(this Span<char> destination, long value, int digits)
    {
        if (value >= 0)
        {
            destination.WritePositiveInt64(value, digits);
            return;
        }

        destination[0] = '-';

        if (value == long.MinValue)
        {
            "9223372036854775808".AsSpan().CopyTo(destination[1..]);
            return;
        }

        destination[1..].WritePositiveInt64(-value, digits - 1);
    }

    /// <summary>
    /// Writes the decimal representation of a non-negative <see cref="long"/> value into the provided <see cref="Span{Char}"/>.
    /// </summary>
    /// <param name="destination">
    /// The destination span that will receive the digits. Its length must be greater than or equal to <paramref name="digits"/>.
    /// </param>
    /// <param name="value">
    /// The non-negative value to write.
    /// </param>
    /// <param name="digits">
    /// The exact number of decimal digits in <paramref name="value"/>.
    /// </param>
    /// <remarks>
    /// Digits are written in reverse order into the destination span, avoiding allocations and formatting overhead.
    /// This method assumes <paramref name="value"/> is non-negative and does not perform validation for performance reasons.
    /// Passing a negative value, an incorrect <paramref name="digits"/> value, or a destination span that is too small will result in incorrect behavior.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WritePositiveInt64(this Span<char> destination, long value, int digits)
    {
        var remaining = (ulong)value;
        int index = digits - 1;

        do
        {
            ulong div = remaining / 10;
            destination[index--] = (char)('0' + (remaining - div * 10));
            remaining = div;
        } while (remaining != 0);
    }
}