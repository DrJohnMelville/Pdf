using System.Linq;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;
using PdfIndirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfIndirectValue;

namespace Melville.Pdf.LowLevel.Writers;

/// <summary>
/// Adds various filters to a DictionaryBuilder that will eventually build a Stream.
/// </summary>
public static class StreamBuilderOperations
{
    /// <summary>
    /// Adds one or more filters to the dictionary builder that will eventually become a stream.
    /// </summary>
    /// <param name="b">The dictionary b</param>
    /// <param name="filters">The filters to add.</param>
    /// <returns>The dictionary builder</returns>
    public static ValueDictionaryBuilder WithFilter(
        in this ValueDictionaryBuilder b, params FilterName[] filters) => 
        b.WithItem(KnownNames.FilterTName, EncodeFilterSelection(filters));

    private static PdfDirectValue EncodeFilterSelection(FilterName[] filters) =>
        filters.Length == 1 ? 
            filters[0]:
            new PdfValueArray(filters.Select(i=>(PdfIndirectValue)(PdfDirectValue)i).ToArray());

    /// <summary>
    /// Adds a parameter object for a single filter to a dictionary builder that will eventually
    /// become a stream.
    /// </summary>
    /// <param name="b">The dictionary builder.</param>
    /// <param name="param">The parameter to add</param>
    /// <returns>The dictionary builder.</returns>
    public static ValueDictionaryBuilder WithFilterParam(
        in this ValueDictionaryBuilder b, PdfIndirectValue param) =>
        b.WithItem(KnownNames.DecodeParmsTName, param);

    /// <summary>
    /// Adds multiple parameter objects for filtera to a dictionary builder that will eventually
    /// become a stream.
    /// </summary>
    /// <param name="b">The dictionary builder.</param>
    /// <param name="param">The parametera to add</param>
    /// <returns>The dictionary builder.</returns>
    public static ValueDictionaryBuilder WithFilterParam(
        in this ValueDictionaryBuilder b, params PdfIndirectValue[] param) =>
        b.WithItem(KnownNames.DecodeParmsTName, new PdfValueArray(param));
}