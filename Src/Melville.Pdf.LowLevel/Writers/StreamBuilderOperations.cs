using System.Linq;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers;

public static class StreamBuilderOperations
{
    public static DictionaryBuilder WithFilter(
        in this DictionaryBuilder b, params FilterName[] filters) => 
        b.WithItem(KnownNames.Filter, EncodeFilterSelection(filters));

    private static PdfObject? EncodeFilterSelection(FilterName[] filters) =>
        filters.Length == 1 ? 
            filters[0]:
            new PdfArray(filters.Select(i=>(PdfObject)(PdfName)i).ToArray());

    public static DictionaryBuilder WithFilterParam(
        in this DictionaryBuilder b, PdfObject? param) =>
        b.WithItem(KnownNames.DecodeParms, param);
    
    public static DictionaryBuilder WithFilterParam(
        in this DictionaryBuilder b, params PdfObject[] param) =>
        b.WithItem(KnownNames.DecodeParms, new PdfArray(param));
}