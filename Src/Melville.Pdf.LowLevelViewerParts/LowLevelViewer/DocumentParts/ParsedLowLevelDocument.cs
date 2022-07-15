using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.References;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts;

public class ParsedLowLevelDocument
{
    public DocumentPart[] Root { get; }
    public CrossReference[] Pages { get; }

    public ParsedLowLevelDocument(DocumentPart[] root, CrossReference[] pages)
    {
        Root = root;
        Pages = pages;
    }
}