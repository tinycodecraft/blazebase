using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Soenneker.Extensions.Arrays.Bytes;

/// <summary>
/// A collection of helpful byte[] extension methods
/// </summary>
public static class ByteArrayExtension
{
    /// <summary>
    /// Converts the specified byte array to a UTF-8 encoded string.
    /// </summary>
    /// <param name="value">The byte array to convert.</param>
    /// <returns>A string representation of the byte array, decoded using UTF-8 encoding.</returns>
    [Pure]
    public static string ToStr(this byte[] value)
    {
        if (value.Length == 0)
            return "";

        return Encoding.UTF8.GetString(value);
    }

    /// <summary>
    /// Converts the specified read-only span of UTF-8 encoded bytes to its equivalent string representation.
    /// </summary>
    /// <param name="value">A read-only span of bytes containing the UTF-8 encoded data to convert.</param>
    /// <returns>A string that represents the decoded UTF-8 text. Returns an empty string if the span is empty.</returns>
    [Pure]
    public static string ToStr(this ReadOnlySpan<byte> value)
    {
        if (value.IsEmpty)
            return "";

        return Encoding.UTF8.GetString(value);
    }

    [Pure]
    public static string ToHex(this byte[] value)
    {
        if (value.Length == 0)
            return "";

        return Convert.ToHexString(value);
    }

    /// <summary>
    /// Converts the specified byte array to its lowercase hexadecimal string representation.
    /// </summary>
    /// <remarks>Each byte is represented by two lowercase hexadecimal characters. The resulting string
    /// contains no separators or prefixes.</remarks>
    /// <param name="value">The array of bytes to convert to a hexadecimal string. Cannot be null.</param>
    /// <returns>A string containing the lowercase hexadecimal representation of the input bytes. Returns an empty string if the
    /// array is empty.</returns>
    [Pure]
    public static string ToHexLower(this byte[] value)
    {
        if (value.Length == 0)
            return "";

        return string.Create(value.Length * 2, value, static (dst, src) =>
        {
            var di = 0;

            for (var i = 0; i < src.Length; i++)
            {
                byte b = src[i];

                int hi = b >> 4;
                int lo = b & 0xF;

                dst[di++] = (char)(hi < 10 ? '0' + hi : 'a' + (hi - 10));
                dst[di++] = (char)(lo < 10 ? '0' + lo : 'a' + (lo - 10));
            }
        });
    }

    /// <summary>
    /// Converts the specified byte array to a Base64-encoded string.
    /// </summary>
    /// <param name="value">The byte array to encode. If null or empty, an empty string is returned.</param>
    /// <returns>A Base64-encoded string.</returns>
    [Pure]
    public static string ToBase64String(this byte[] value)
    {
        if (value.Length == 0)
            return "";

        return Convert.ToBase64String(value);
    }

    /// <summary>
    /// Converts the specified read-only span of bytes to its equivalent string representation encoded with base-64
    /// digits.
    /// </summary>
    /// <param name="value">The read-only span of bytes to convert to a base-64 encoded string.</param>
    /// <returns>A string containing the base-64 encoded representation of the input bytes, or an empty string if the input span
    /// is empty.</returns>
    [Pure]
    public static string ToBase64String(this ReadOnlySpan<byte> value)
    {
        if (value.IsEmpty)
            return "";

        return Convert.ToBase64String(value);
    }

    /// <summary>
    /// Converts the byte array into a <see cref="MemoryStream"/>.
    /// </summary>
    /// <param name="value">The byte array to convert.</param>
    /// <returns>A <see cref="MemoryStream"/> containing the byte array data.</returns>
    [Pure]
    public static MemoryStream ToStream(this byte[] value)
    {
        return new MemoryStream(value);
    }

    /// <summary>
    /// Determines whether the byte array is null or empty.
    /// </summary>
    /// <param name="value">The byte array to check.</param>
    /// <returns>True if the byte array is null or empty; otherwise, false.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEmpty(this byte[]? value)
    {
        return value is null || value.Length == 0;
    }
}