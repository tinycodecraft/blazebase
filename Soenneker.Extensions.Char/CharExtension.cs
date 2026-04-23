using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Soenneker.Extensions.Char;

/// <summary>
/// A collection of helpful <see cref="char"/> extension methods, optimized for fast ASCII checks
/// with safe Unicode fallbacks where needed.
/// </summary>
public static class CharExtension
{
    // ASCII 0..63
    private const ulong _tokenSepMaskLo =
        (1UL << 9) | // \t
        (1UL << 10) | // \n
        (1UL << 13) | // \r
        (1UL << 32) | // ' '
        (1UL << 45) | // '-'
        (1UL << 46) | // '.'
        (1UL << 47) | // '/'
        (1UL << 58) | // ':'
        (1UL << 59);  // ';'

    // ASCII 64..127 (bit index = code - 64)
    private const ulong _tokenSepMaskHi =
        (1UL << (92 - 64)) | // '\\' (28)
        (1UL << (95 - 64));  // '_'  (31)

    /// <summary>
    /// Determines whether the character is within the 7-bit ASCII range (U+0000 to U+007F).
    /// </summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns><see langword="true"/> if the character is ASCII; otherwise, <see langword="false"/>.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAscii(this char c) => c <= 0x7F;

    /// <summary>
    /// Determines whether the character is an ASCII uppercase letter ('A' through 'Z').
    /// </summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns><see langword="true"/> if the character is an ASCII uppercase letter; otherwise, <see langword="false"/>.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiUpper(this char c) => (uint)(c - 'A') <= 25;

    /// <summary>
    /// Determines whether the character is an ASCII lowercase letter ('a' through 'z').
    /// </summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns><see langword="true"/> if the character is an ASCII lowercase letter; otherwise, <see langword="false"/>.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiLower(this char c) => (uint)(c - 'a') <= 25;

    /// <summary>
    /// Determines whether the character is an ASCII letter ('A'–'Z' or 'a'–'z').
    /// </summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns><see langword="true"/> if the character is an ASCII letter; otherwise, <see langword="false"/>.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiLetter(this char c) => (uint)((c | 0x20) - 'a') <= 25;

    /// <summary>
    /// Determines whether the character is an ASCII digit ('0' through '9').
    /// </summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns><see langword="true"/> if the character is an ASCII digit; otherwise, <see langword="false"/>.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiDigit(this char c) => (uint)(c - '0') <= 9;

    /// <summary>
    /// Determines whether the character is ASCII alphanumeric ('A'–'Z', 'a'–'z', or '0'–'9').
    /// </summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns><see langword="true"/> if the character is ASCII alphanumeric; otherwise, <see langword="false"/>.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiLetterOrDigit(this char c)
    {
        // digits first often wins for numeric-heavy inputs (IDs), but either is fine
        return (uint)(c - '0') <= 9 || (uint)((c | 0x20) - 'a') <= 25;
    }

    /// <summary>
    /// Determines whether the character is ASCII whitespace.
    /// This checks for: space (U+0020), tab (U+0009), LF (U+000A),
    /// VT (U+000B), FF (U+000C), and CR (U+000D).
    /// </summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns><see langword="true"/> if the character is one of the ASCII whitespace characters; otherwise, <see langword="false"/>.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiWhiteSpace(this char c)
    {
        uint uc = c;
        return uc <= 0x20u && (uc == 0x20u || uc - 0x09u <= 4u); // ' ' or 0x09..0x0D
    }

    /// <summary>
    /// Converts an ASCII lowercase letter ('a'–'z') to its uppercase equivalent.
    /// Non-ASCII or non-lowercase characters are returned unchanged.
    /// </summary>
    /// <param name="c">The character to convert.</param>
    /// <returns>The uppercase ASCII equivalent if applicable; otherwise, the original character.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char ToAsciiUpper(this char c)
    {
        uint uc = c;
        return uc - 'a' <= 25u ? (char)(uc & ~0x20u) : c;
    }

    /// <summary>
    /// Converts an ASCII uppercase letter ('A'–'Z') to its lowercase equivalent.
    /// Non-ASCII or non-uppercase characters are returned unchanged.
    /// </summary>
    /// <param name="c">The character to convert.</param>
    /// <returns>The lowercase ASCII equivalent if applicable; otherwise, the original character.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char ToAsciiLower(this char c)
    {
        uint uc = c;
        return uc - 'A' <= 25u ? (char)(uc | 0x20u) : c;
    }

    /// <summary>
    /// Converts the specified Unicode character to its uppercase equivalent using the invariant culture.
    /// </summary>
    /// <remarks>This method performs a fast conversion for ASCII characters and delegates to the standard
    /// library for non-ASCII characters. The conversion is culture-invariant and does not depend on the current
    /// locale.</remarks>
    /// <param name="c">The Unicode character to convert to uppercase.</param>
    /// <returns>The uppercase equivalent of the specified character, or the original character if it has no uppercase mapping.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char ToUpperInvariant(this char c)
    {
        uint uc = c;

        if (uc <= 0x7Fu)
            return uc - 'a' <= 25u ? (char)(uc & ~0x20u) : c;

        return char.ToUpperInvariant(c);
    }

    /// <summary>
    /// Converts the specified Unicode character to its lowercase equivalent using the invariant culture.
    /// </summary>
    /// <remarks>This method performs a culture-insensitive conversion. It is suitable for scenarios where
    /// predictable, culture-independent casing is required, such as protocol or file format processing.</remarks>
    /// <param name="c">The Unicode character to convert to lowercase.</param>
    /// <returns>A character that is the lowercase equivalent of the input character, or the original character if it has no
    /// lowercase equivalent.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char ToLowerInvariant(this char c)
    {
        uint uc = c;

        if (uc <= 0x7Fu)
            return uc - 'A' <= 25u ? (char)(uc | 0x20u) : c;

        return char.ToLowerInvariant(c);
    }

    /// <summary>
    /// Determines whether the character is a letter or digit using a fast ASCII path,
    /// falling back to <see cref="char.IsLetterOrDigit(char)"/> for non-ASCII input.
    /// </summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns><see langword="true"/> if the character is a letter or digit; otherwise, <see langword="false"/>.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLetterOrDigitFast(this char c)
    {
        uint uc = c;
        return uc <= 0x7Fu
            ? uc - '0' <= 9u || (uc | 0x20u) - 'a' <= 25u
            : char.IsLetterOrDigit(c);
    }

    /// <summary>
    /// Determines whether the character is whitespace using a fast ASCII path
    /// (space, tab, LF, VT, FF, CR), falling back to <see cref="char.IsWhiteSpace(char)"/>
    /// for non-ASCII input.
    /// </summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns><see langword="true"/> if the character is whitespace; otherwise, <see langword="false"/>.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsWhiteSpaceFast(this char c)
    {
        uint uc = c;

        if (uc <= 0x20u)
            return uc == 0x20u || uc - 0x09u <= 4u;

        if (uc <= 0x7Fu)
            return false;

        return char.IsWhiteSpace(c);
    }

    /// <summary>
    /// Determines whether the character is a digit using a fast ASCII path,
    /// falling back to <see cref="char.IsDigit(char)"/> for non-ASCII input.
    /// </summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns><see langword="true"/> if the character is a digit; otherwise, <see langword="false"/>.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDigitFast(this char c)
    {
        uint uc = c;
        return uc <= 0x7Fu ? uc - '0' <= 9u : char.IsDigit(c);
    }

    /// <summary>
    /// Determines whether the character is a letter using a fast ASCII path,
    /// falling back to <see cref="char.IsLetter(char)"/> for non-ASCII input.
    /// </summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns><see langword="true"/> if the character is a letter; otherwise, <see langword="false"/>.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLetterFast(this char c)
    {
        uint uc = c;
        return uc <= 0x7Fu
            ? (uc | 0x20u) - 'a' <= 25u
            : char.IsLetter(c);
    }

    /// <summary>
    /// Determines whether the character is uppercase using a fast ASCII path,
    /// falling back to <see cref="char.IsUpper(char)"/> for non-ASCII input.
    /// </summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns><see langword="true"/> if the character is uppercase; otherwise, <see langword="false"/>.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsUpperFast(this char c)
    {
        uint uc = c;
        return uc <= 0x7Fu
            ? uc - 'A' <= 25u
            : char.IsUpper(c);
    }

    /// <summary>
    /// Determines whether the character is lowercase using a fast ASCII path,
    /// falling back to <see cref="char.IsLower(char)"/> for non-ASCII input.
    /// </summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns><see langword="true"/> if the character is lowercase; otherwise, <see langword="false"/>.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLowerFast(this char c)
    {
        uint uc = c;
        return uc <= 0x7Fu
            ? uc - 'a' <= 25u
            : char.IsLower(c);
    }

    /// <summary>
    /// Determines whether the character is considered a token separator.
    /// Separators include ASCII whitespace, underscore, hyphen, period, forward slash,
    /// backslash, colon, and semicolon.
    /// </summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns><see langword="true"/> if the character is a token separator; otherwise, <see langword="false"/>.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsTokenSeparator(this char c)
    {
        uint uc = c;

        if (uc > 0x7Fu) 
            return false;

        var idx = (int)uc;
        return idx < 64
            ? ((_tokenSepMaskLo >> idx) & 1UL) != 0
            : ((_tokenSepMaskHi >> (idx - 64)) & 1UL) != 0;
    }

    /// <summary>
    /// Determines whether the specified character is an ASCII newline character (line feed '\n' or carriage return
    /// '\r').
    /// </summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns>true if the character is '\n' or '\r'; otherwise, false.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiNewLine(this char c)
        => c is '\n' or '\r';

    /// <summary>
    /// Determines whether the specified character is recognized as a Unicode newline character.
    /// </summary>
    /// <remarks>This method checks for the most common Unicode newline characters, including line feed (LF),
    /// carriage return (CR), next line (NEL), line separator (LS), and paragraph separator (PS). It is optimized for
    /// performance and suitable for high-throughput scenarios such as text parsing.</remarks>
    /// <param name="c">The character to evaluate.</param>
    /// <returns>true if the character is a line break character (LF, CR, NEL, LS, or PS); otherwise, false.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNewLineFast(this char c)
        => c is '\n' or '\r' or '\u0085' or '\u2028' or '\u2029';
}