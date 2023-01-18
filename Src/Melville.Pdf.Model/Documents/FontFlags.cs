using System;

namespace Melville.Pdf.Model.Documents;

/// <summary>
/// This flags enum matches the FontFlags definition in the PDF Spec.  Due to a bug in the spec
/// some combinations like Symbolic | NonSumbolic are not meaningful.
/// </summary>
[Flags]
public enum FontFlags: int
{
    /// <summary>
    /// None of the other flags are set.
    /// </summary>
    None =  0,
    /// <summary>
    /// All characters are the same width
    /// </summary>
    FixedPitch = 1,
    /// <summary>
    /// Font uses serifs on the ends of major lines
    /// </summary>
    Serif = 1 << 1,
    /// <summary>
    /// Font does not display natural language symbols
    /// </summary>
    Symbolic = 1 << 2,
    /// <summary>
    /// Font has the appearance of cursive script
    /// </summary>
    Script = 1 << 3,
    /// <summary>
    /// Font displays roman characters at their typical code points
    /// </summary>
    NonSymbolic = 1 << 5,
    /// <summary>
    /// Characters are skewed leading to an Italic font
    /// </summary>
    Italic = 1 <<6,
    /// <summary>
    /// Font displays lower case letters as capital letters.
    /// </summary>
    AllCap = 1 << 16,
    /// <summary>
    /// All letters are shown with upper case shapes but lower case size.
    /// </summary>
    SmallCap = 1 << 17,
    /// <summary>
    /// Font is a bold font.
    /// </summary>
    ForceBold = 1<<18
}