using System;
using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Objects2;

namespace Melville.Pdf.LowLevel.Model.Objects;

/// <summary>
/// This class defines helper methods for parsing items where an array of one element can be
/// replaces with just the single element.
/// </summary>
public static class PdfObjectToListOperations
{

    /// <summary>
    /// At a number of places in the Pdf Spec an array with a single element can be replaced by
    /// just the single element.  This method resolves that ambiguity.
    /// Arrays map to their elements.
    /// PdfNull objects map to an empty list.
    /// Everything else maps to a synthesized array that contains the element.
    /// This method does not follow any references.
    /// </summary>
    /// <param name="item">The item to follow.</param>
    /// <returns>A list of PdfObjects with the semantics given above.</returns>
    public static IReadOnlyList<PdfIndirectValue> ObjectAsUnresolvedList(this PdfDirectValue item) =>
        item switch
        {
            {IsNull:true} => Array.Empty<PdfIndirectValue>(),
            _ when item.TryGet(out PdfValueArray valueArray) => valueArray.RawItems,
            _ => new [] {(PdfIndirectValue)item}
        };
}