namespace Melville.Fonts.SfntParsers;

/// <summary>
/// This is a class of constants with common SFnt table names
/// </summary>
public class SFntTableName
{
    /// <summary>
    /// Table name 'cmap'
    /// </summary>
    public const uint CMap = 0x636d6170;

    /// <summary>
    /// Table name 'head'
    /// </summary>
    public const uint Head = 0x68656164;

    /// <summary>
    /// Table name 'hhea'
    /// </summary>
    public const uint HorizontalHeadder = 0x68686561;

    /// <summary>
    /// Table name 'maxp'
    /// </summary>
    public const uint MaximumProfile = 0x6D617870;

    /// <summary>
    /// Table name 'htmx"
    /// </summary>
    public const uint HorizontalMetrics = 0x686D7478;

    /// <summary>
    /// Table name 'loca'
    /// </summary>
    public const uint GlyphLocations = 0x6C6F6361;

    /// <summary>
    /// Table name 'glyf'
    /// </summary>
    public const uint GlyphData = 0x676C7966;

    /// <summary>
    /// Table name 'CFF '
    /// </summary>
    public const uint CFF = 0x43464620;
}