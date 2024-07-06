namespace Melville.Fonts;

/// <summary>
/// This class maps plafrom and encoding codes to strings for use in displaying Cmap names in the UI
/// </summary>
public static class CmapPlatFormAndEncodingNames
{
    /// <summary>
    /// Get a standard platform name from its CMAP encoding
    /// </summary>
    /// <param name="platform">The cmap platform value</param>
    /// <returns>The corresponding english platform name</returns>
    public static string PlatformName(int platform) => platform switch
    {
        0 => "Unicode",
        1 => "Macintosh",
        2 => "ISO",
        3 => "Windows",
        4 => "Custom",
        _ => "Unknown"
    };

    /// <summary>
    /// Gets an encoding name for a given Cmap platform and encoding 
    /// </summary>
    /// <param name="platform"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static string EncodingName(int platform, int encoding) => (platform, encoding) switch
    {
        (0, 0) => "Unicode 1.0",
        (0, 1) => "Unicode 1.1",
        (0, 2) => "ISO 10646",
        (0, 3) => "Unicode 2.0 BMP Only",
        (0, 4) => "Unicode 2.0 Full",
        (0, 5) => "Unicode Variation Sequences",
        (0, 6) => "Unicode Full",
        (1, 0) => "7-bit ASCII",
        (2, 1) => "ISO 10646",
        (2, 2) => "ISO 8859-1",
        (3, 0) => "Symbol",
        (3, 1) => "Unicode BMP",
        (3, 2) => "Shift-JIS",
        (3, 3) => "PRC",
        (3, 4) => "Big 5",
        (3, 5) => "Wansung",
        (3, 6) => "Johab",
        (3, 7) => "Reserved",
        (3, 8) => "Reserved",
        (3, 9) => "Reserved",
        (3, 10) => "Unicode Full",
        (4, _) => "OpenType Windows NT Compatibility Mapping",
        _ => "Unknown"
    };
}