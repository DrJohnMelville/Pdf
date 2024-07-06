namespace Melville.Fonts.SfntParsers.TableDeclarations.Heads;

/// <summary>
/// MacStyles field in the SFnt Header structure
/// </summary>
[Flags]
public enum MacStyles: ushort
{
    /// <summary>
    /// No style
    /// </summary>
    None = 0,
    /// <summary>
    /// Bold Text
    /// </summary>
    Bold = 1,
    /// <summary>
    /// Slanted or Italic Text
    /// </summary>
    Italic = 2,
    /// <summary>
    /// Underlined Text
    /// </summary>
    Underline = 4,
    /// <summary>
    /// Draw outlines of text only
    /// </summary>
    Outline = 8,
    /// <summary>
    /// Draws text with a shadow.
    /// </summary>
    Shadow = 16,
    /// <summary>
    /// Condensed text, I presume this is a narrow font.
    /// </summary>
    Condensed = 32,
    /// <summary>
    /// Extended text (I presume this is a wide font)
    /// </summary>
    Extended = 64
}