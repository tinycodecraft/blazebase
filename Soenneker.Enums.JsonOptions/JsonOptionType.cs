using Soenneker.Gen.EnumValues;

namespace Soenneker.Enums.JsonOptions;

/// <summary>
/// Represents different JSON option types.
/// </summary>
[EnumValue]
public sealed partial class JsonOptionType
{
    /// <summary>
    /// Web defaults, non-strict
    /// </summary>
    public static readonly JsonOptionType Web = new(0);

    /// <summary>
    /// Non-camel case
    /// </summary>
    public static readonly JsonOptionType General = new(1);

    /// <summary>
    /// Non-camel case with indentation WITHOUT escaping. WARNING Dangerous! Do not use unless for internal uses!
    /// </summary>
    /// <remarks>https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-character-encoding</remarks>
    public static readonly JsonOptionType Pretty = new(2);

    /// <summary>
    /// Non-camel case with indentation, WITH escaping. Safe for output.
    /// </summary>
    /// <remarks>https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-character-encoding</remarks>
    public static readonly JsonOptionType PrettySafe = new(3);
}
