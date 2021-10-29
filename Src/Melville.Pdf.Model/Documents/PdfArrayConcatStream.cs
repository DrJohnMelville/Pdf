using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.Model.Documents;

public class PdfArrayConcatStream : ConcatStreamBase
{
    private readonly IEnumerator<ValueTask<PdfObject>> source;
    private Stream? currentSource;

    public PdfArrayConcatStream(PdfArray source)
    {
        this.source = source.GetEnumerator();
    }

    protected override async ValueTask<Stream?> GetNextStream()
    {
        if (!source.MoveNext()) return null;
        var stream = (await source.Current) as PdfStream ??
                  throw new PdfParseException("Content array should contain only streams");
        return await stream.StreamContentAsync();
    }
}