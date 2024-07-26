namespace Melville.Fonts.SfntParsers.TableDeclarations.Names;

/// <summary>
/// This enum is the type of a name in the SFnt Name table
/// </summary>
public enum SfntNameKey : ushort
{
    /// <summary>
    /// Copyright notice for the font.
    /// </summary>
    CopyrightNotice = 0,
    /// <summary>
    /// Family  name -- family should contain only the four standard subfonts
    /// </summary>
    FontFamilyName = 1,
    /// <summary>
    /// Bold and or oblique suff3ix to the family name
    /// </summary>
    FontSubfamilyName = 2,
    /// <summary>
    /// A unique identifier for the font
    /// </summary>
    UniqueFontIdentifier = 3,
    /// <summary>
    /// A full font name, usually combining items 1 and 2
    /// </summary>
    FullFontName = 4,
    /// <summary>
    /// Version string for the font program
    /// </summary>
    VersionString = 5,
    /// <summary>
    /// Name of the font when used in a postscript interpreter.
    /// </summary>
    PostScriptName = 6,
    /// <summary>
    /// Trademark notice for the font, which is distinct from copyright
    /// </summary>
    Trademark = 7,
    /// <summary>
    /// Manufacturer name
    /// </summary>
    ManufacturerName = 8,
    /// <summary>
    /// Font designer name
    /// </summary>
    DesignerName = 9,
    /// <summary>
    /// Description of the font from the font designer.
    /// </summary>
    Description = 10,
    /// <summary>
    /// Url for the font vendor including the protocol, e.g. http://, ftp://
    /// </summary>
    VendorUrl = 11,
    /// <summary>
    /// Url for the typeface designer including the protocol
    /// </summary>
    DesignerUrl = 12,
    /// <summary>
    /// Plaintext license information for this font
    /// </summary>
    LicenseDescription = 13,
    /// <summary>
    /// Url where additional license information can be found.
    /// </summary>
    LicenseInfoUrl = 14,
    /// <summary>
    /// A family name that imposes no restrictions on the fonts contained within it.
    /// </summary>
    TypographicFamilyName = 16,
    /// <summary>
    /// Subfamily name within the font family defined in item 16/
    /// </summary>
    TypographicSubfamilyName = 17,
    /// <summary>
    /// Appearance of the font name in macintosh font menus
    /// </summary>
    CompatibleFull = 18,
    /// <summary>
    /// Sample text that the designed thinks will highlight this font.
    /// </summary>
    SampleText = 19,
    /// <summary>
    /// Name to use in the postscript findfont function to find this font.
    /// </summary>
    PostScriptCid = 20,
    /// <summary>
    /// A WWS family name for use on OS/2 systems
    /// </summary>
    WwsFamilyName = 21,
    /// <summary>
    /// A WWS subfamily name for use on OS/2 systems
    /// </summary>
    WwsSubfamilyName = 22,
    /// <summary>
    /// Suggested font palette for displaying this font against a light background
    /// </summary>
    LightBackgroundPalette = 23,
    /// <summary>
    /// Suggested font palette for displaying this font against a dark background
    /// </summary>
    DarkBackgroundPalette = 24,
    /// <summary>
    /// Font name prefix for variations fonts in postscript
    /// </summary>
    VariationsPostScriptNamePrefix = 25
}