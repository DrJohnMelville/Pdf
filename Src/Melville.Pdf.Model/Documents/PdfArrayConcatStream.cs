using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.StreamFilters;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
namespace Melville.Pdf.Model.Documents;

/// <summary>
/// This class handles the case where a PDFArray defines a sequence of contentStreams.  We can avoid the extra array
/// allocation by enumerating the PdfArray directly and then extracting the content stream from each element as we need
/// it.
/// </summary>
internal class PdfArrayConcatStream : ConcatStreamBase
{
    private readonly IEnumerator<ValueTask<PdfDirectValue>> source;

    public PdfArrayConcatStream(PdfValueArray source)
    {
        this.source = source.GetEnumerator();
    }

    protected override async ValueTask<Stream?> GetNextStreamAsync()
    {
        if (!source.MoveNext()) return null;
        var stream = (await source.Current).Get<PdfValueStream>();
        return await stream.StreamContentAsync().CA();
    }
}