using System.Linq;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Writers;

public static class StreamBuilderOperations
{
    /// <summary>
    /// Adds one or more filters to the dictionary builder that will eventually become a stream.
    /// </summary>
    /// <param name="b">The dictionary b</param>
    /// <param name="filters">The filters to add.</param>
    /// <returns>The dictionary builder</returns>
    public static DictionaryBuilder WithFilter(
        in this DictionaryBuilder b, params FilterName[] filters) => 
        b.WithItem(KnownNames.Filter, EncodeFilterSelection(filters));

    private static PdfObject? EncodeFilterSelection(FilterName[] filters) =>
        filters.Length == 1 ? 
            filters[0]:
            new PdfArray(filters.Select(i=>(PdfObject)(PdfName)i).ToArray());

    /// <summary>
    /// Adds a parameter object for a single filter to a dictionary builder that will eventually
    /// become a stream.
    /// </summary>
    /// <param name="b">The dictionary builder.</param>
    /// <param name="param">The parameter to add</param>
    /// <returns>The dictionary builder.</returns>
    public static DictionaryBuilder WithFilterParam(
        in this DictionaryBuilder b, PdfObject? param) =>
        b.WithItem(KnownNames.DecodeParms, param);

    /// <summary>
    /// Adds multiple parameter objects for filtera to a dictionary builder that will eventually
    /// become a stream.
    /// </summary>
    /// <param name="b">The dictionary builder.</param>
    /// <param name="param">The parametera to add</param>
    /// <returns>The dictionary builder.</returns>
    public static DictionaryBuilder WithFilterParam(
        in this DictionaryBuilder b, params PdfObject[] param) =>
        b.WithItem(KnownNames.DecodeParms, new PdfArray(param));
}