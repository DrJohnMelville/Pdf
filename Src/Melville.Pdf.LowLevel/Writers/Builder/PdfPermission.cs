using System;

namespace Melville.Pdf.LowLevel.Writers.Builder;

/// <summary>
/// Represents options that are forbidden for an encrypted PDF file.  Notice that permissions are negative, so permissions of 0
/// lets the user do anything.
/// </summary>
[Flags]
public enum PdfPermission
{
    /// <summary>
    /// Allow users unrestricted access to the document.
    /// </summary>
    None = 0,
    /// <summary>
    /// Print the document to a system printer.
    /// </summary>
    PrintDocument = 1 << 2,
    /// <summary>
    /// Modify the document
    /// </summary>
    ModifyDocument = 1 << 3,
    /// <summary>
    /// Copy from the document to the clipboard.
    /// </summary>
    CopyFrom = 1 << 4,
    /// <summary>
    /// Add annotations to the document. (Which Melville.PDF does not support as of 1/13/2023.)
    /// </summary>
    Annotate = 1 << 5,
    /// <summary>
    /// Fill out fields in PDF forms
    /// </summary>
    FillForms = 1 << 8,
    /// <summary>
    /// Indicates that screen readers can extract text.  (In PDF 2.0 this restriction is no longer enforced.)
    /// </summary>
    ExtractTextForDisabilitis = 1 << 9,
    /// <summary>
    /// Assemble multiple documents into a single document.
    /// </summary>
    AssembleDocument = 1 << 10,
    /// <summary>
    /// Print a copy that does not have degraded resolution.
    /// </summary>
    PrintFaithfulCopy = 1 << 11,
}