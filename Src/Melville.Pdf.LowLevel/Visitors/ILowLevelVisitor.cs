using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Visitors;

/// <summary>
/// This interface is used to define visitors for the PdfLowLevelObjects
/// </summary>
/// <typeparam name="T">Output type of the visitor</typeparam>
[MacroItem("PdfArray")]
[MacroItem("PdfBoolean")]
[MacroItem("PdfDictionary")]
[MacroItem("PdfTokenValues")]
[MacroItem("PdfIndirectObject")]
[MacroItem("PdfName")]
[MacroItem("PdfInteger")]
[MacroItem("PdfDouble")]
[MacroItem("PdfString")]
[MacroItem("PdfStream")]
[MacroCode("""

            /// <summary>
            /// Visit a ~0~.
            /// </summary>
            /// <param name="item">The ~0~ to visit </param>
            /// <returns>A result of the visit operation.</returns>
            T Visit(~0~ item); 
        """)]
public partial interface ILowLevelVisitor<out T>
{
    /// <summary>
    /// Visit a PdfIndirectObject as a top level object definition.
    /// </summary>
    /// <param name="item">The PdfIndirectObject to visit </param>
    /// <returns>A return value.</returns>
    T VisitTopLevelObject(PdfIndirectObject item);
}