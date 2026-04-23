using Soenneker.Gen.EnumValues;

namespace Soenneker.Enums.ContentKinds;

/// <summary>
/// An enum for various content types.
/// </summary>
[EnumValue<string>]
public sealed partial class ContentKind
{
    public static readonly ContentKind Json = new(nameof(Json));

    public static readonly ContentKind XmlOrHtml = new(nameof(XmlOrHtml));

    public static readonly ContentKind Text = new(nameof(Text));

    public static readonly ContentKind Binary = new(nameof(Binary));

    public static readonly ContentKind Unknown = new(nameof(Unknown));
}