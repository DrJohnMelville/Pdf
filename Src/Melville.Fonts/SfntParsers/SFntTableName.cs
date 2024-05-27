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
}