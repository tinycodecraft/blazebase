using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Soenneker.Extensions.Spans.Bytes;

/// <summary>
/// Various helpful byte span extension methods
/// </summary>
public static class SpanByteExtension
{
    /// <summary>
    /// Overwrites the contents of the specified span with zeros in a manner designed to prevent the data from being
    /// recovered from memory. Equivalent to <see cref="CryptographicOperations.ZeroMemory(Span{byte})"/>.
    /// </summary>
    /// <remarks>This method is intended for securely erasing sensitive data, such as cryptographic keys or
    /// passwords, from memory. It uses techniques to minimize the risk that the data remains accessible after clearing,
    /// even in the presence of compiler optimizations.</remarks>
    /// <param name="span">The span of bytes to be securely cleared. All elements in the span will be set to zero.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SecureZero(this Span<byte> span)
    {
        CryptographicOperations.ZeroMemory(span);
    }
}