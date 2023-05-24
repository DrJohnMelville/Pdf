using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    /// null or PdfNull objects map to an empty list.
    /// Everything else maps to a synthesized array that contains the element.
    /// This method does not follow any references.
    /// </summary>
    /// <param name="item">The item to follow.</param>
    /// <returns>A list of PdfObjects with the semantics given above.</returns>
    public static IReadOnlyList<PdfObject> ObjectAsUnresolvedList(this PdfObject? item)
    {
        return item switch
        {
            PdfArray arr => arr.RawItems,
            {} x and (PdfBoolean or not PdfTokenValues)=> new []{x},
            _ => Array.Empty<PdfObject>(),
        };
    }

    /// <summary>
    /// At a number of places in the PDF spec a single item array can be replaced by just the item. This method resolves this,
    /// a given pdf object is converted into an array of a give, PdfObject child.  IF it is a bare object an array is synthesized.
    /// All indirect references are resolved.
    /// </summary>
    /// <typeparam name="T">Desired PdfObject child type</typeparam>
    /// <param name="source">A PdfObject or PdfArray</param>
    /// <returns>A C# array of PdfObjects that implements the semantics above.</returns>
    public static ValueTask<T[]> ObjectAsResolvedListAsync<T>(this PdfObject source) where T : PdfObject => source switch
    {
        T item => new(new T[] { item }),
        PdfArray arr => arr.AsAsync<T>(),
        _ => new(Array.Empty<T>())
    };
}