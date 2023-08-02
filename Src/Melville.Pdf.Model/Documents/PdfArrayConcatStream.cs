using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.StreamFilters;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Documents;

/// <summary>
/// This class handles the case where a PDFArray defines a sequence of contentStreams.  We can avoid the extra array
/// allocation by enumerating the PdfArray directly and then extracting the content stream from each element as we need
/// it.
/// </summary>
internal class PdfArrayConcatStream : ConcatStreamBase
{
    private readonly IEnumerator<ValueTask<PdfDirectObject>> source;

    public PdfArrayConcatStream(PdfArray source)
    {
        this.source = source.GetEnumerator();
    }

    protected override async ValueTask<Stream?> GetNextStreamAsync()
    {
        if (!source.MoveNext()) return null;
        var stream = (await source.Current).Get<PdfStream>();
        return await stream.StreamContentAsync().CA();
    }
}