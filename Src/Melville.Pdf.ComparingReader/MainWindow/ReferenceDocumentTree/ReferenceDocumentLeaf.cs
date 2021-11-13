using Melville.Pdf.ReferenceDocuments.Infrastructure;

namespace Melville.Pdf.ComparingReader.MainWindow.ReferenceDocumentTree;

public class ReferenceDocumentLeaf: ReferenceDocumentNode
{
    public IPdfGenerator Document { get; }
    public override string ShortName => Document.Prefix[1..];
    public string LongName => Document.HelpText;

    public ReferenceDocumentLeaf(IPdfGenerator document)
    {
        this.Document = document;
    }
}