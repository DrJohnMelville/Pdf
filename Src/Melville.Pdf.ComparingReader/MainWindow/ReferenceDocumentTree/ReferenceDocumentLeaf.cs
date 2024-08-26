using System.Threading.Tasks;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Streams;
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

    public async ValueTask<IMultiplexSource> GetDocumentAsync()
    {
        var buffer = WritableBuffer.Create();
        using var writer = buffer.WritingStream();
        await document.WritePdfAsync(writer);
        return buffer;
    }
}