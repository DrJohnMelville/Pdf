using System.Threading.Tasks;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.ReferenceDocuments.Infrastructure;

namespace Melville.Pdf.ComparingReader.MainWindow.ReferenceDocumentTree;

public class ReferenceDocumentLeaf: ReferenceDocumentNode
{
    private readonly IPdfGenerator document;
    public override string ShortName => document.Prefix[1..];
    public string LongName => document.HelpText;

    public ReferenceDocumentLeaf(IPdfGenerator document)
    {
        this.document = document;
    }

    public async ValueTask<MultiBufferStream> GetDocument()
    {
        var stream = new MultiBufferStream();
        await document.WritePdfAsync(stream);
        return stream;
    }
}