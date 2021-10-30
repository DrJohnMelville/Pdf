using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers;

public static class StreamBuilderOperations
{
    public static DictionaryBuilder WithFilter(
        in this DictionaryBuilder b, FilterName? filter) => b.WithItem(KnownNames.Filter, filter);
    public static DictionaryBuilder WithFilter(
        in this DictionaryBuilder b, params FilterName[] filters) => 
        b.WithItem(KnownNames.Filter, new PdfArray((IReadOnlyList<FilterName>)filters));
    public static DictionaryBuilder WithFilterParam(
        in this DictionaryBuilder b, PdfObject? param) =>
        b.WithItem(KnownNames.DecodeParms, param);
    public static DictionaryBuilder WithFilterParam(
        in this DictionaryBuilder b, params PdfObject[] param) =>
        b.WithItem(KnownNames.DecodeParms, new PdfArray(param));
}