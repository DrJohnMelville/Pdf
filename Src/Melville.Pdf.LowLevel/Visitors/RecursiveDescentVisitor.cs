using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Visitors;

/// <summary>
/// This is an abstract recursive descent visitor class.  It recursively calls into dictionaries and arrays.
/// </summary>
/// <typeparam name="T">The result type of the visitation.</typeparam>
public abstract partial class RecursiveDescentVisitor<T>: ILowLevelVisitor<T>
{
    /// <summary>
    /// This is a default visitation method.  Any method not otherwise defined will visit this method.
    /// </summary>
    /// <param name="item">The visitation result</param>
    /// <returns></returns>
    protected virtual T VisitAny(PdfObject item) => default!;
    /// <summary>
    /// A default visitation method for all number types.
    /// </summary>
    /// <param name="item">The item to visit</param>
    /// <returns>The visitation result.</returns>
    protected virtual T VisitNumber(PdfNumber item) => VisitAny(item);


    /// <inheritdoc />
    [MacroItem("PdfBoolean", "PdfTokenValues")]
    [MacroItem("PdfStream", "PdfDictionary")]
    [MacroCode("""

        ///<inheritdoc />
        public virtual T Visit(~0~ item) => Visit((~1~)item);
        """)]
    public virtual T VisitTopLevelObject(PdfIndirectObject item) => VisitAny(item);

    /// <inheritdoc />
    [MacroItem("PdfName", "VisitAny")]
    [MacroItem("PdfIndirectObject", "VisitAny")]
    [MacroItem("PdfTokenValues", "VisitAny")]
    [MacroItem("PdfString", "VisitAny")]
    [MacroItem("PdfInteger", "VisitNumber")]
    [MacroItem("PdfDouble", "VisitNumber")]
    [MacroCode("""

        ///<inheritdoc />
        public virtual T Visit(~0~ item) => ~1~(item);
        """)]
    public virtual T Visit(PdfArray item)
    {
        var ret = VisitAny(item);
        foreach (var element in item.RawItems)
        {
            element.Visit(this);
        }

        return ret;
    }

    /// <inheritdoc />
    public virtual T Visit(PdfDictionary item)
    {
        var ret = VisitAny(item);
        foreach (var pair in item.RawItems)
        {
            VisitDictionaryPair(pair.Key, pair.Value);
        }

        return ret;
    }

    /// <summary>
    /// Visits a key/value pair in a dictionary.
    /// </summary>
    /// <param name="key">The key in the pair</param>
    /// <param name="item">The value in the pair</param>
    /// <returns>The result of the visit operation</returns>
    protected virtual T VisitDictionaryPair(PdfName key, PdfObject item)
    {
        key.Visit(this);
        return item.Visit(this);
    }
}