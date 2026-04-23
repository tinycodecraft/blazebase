using System.Globalization;

namespace Soenneker.Culture.English.US;

/// <summary>
/// A cache of CultureInfo.GetCultureInfo for en-US.
/// </summary>
public static class CultureEnUsCache
{
    /// <summary>
    /// The cached <see cref="CultureInfo"/> instance for "en-US" (English - United States).
    /// </summary>
    public static readonly CultureInfo Instance = CultureInfo.GetCultureInfo("en-US");
}
