using Melville.Pdf.ReferenceDocuments.Infrastructure;

namespace Melville.Pdf.ComparingReader.MainWindow.ReferenceDocumentTree;

public class ReferenceDocumentLeaf: ReferenceDocumentNode
{
    private IPdfGenerator document;
    public override string ShortName => document.Prefix[1..];
    public string LongName => document.HelpText;

    public ReferenceDocumentLeaf(IPdfGenerator document)
    {
        this.document = document;
    }
}