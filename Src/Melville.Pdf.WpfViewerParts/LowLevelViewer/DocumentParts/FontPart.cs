using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.WpfViewerParts.LowLevelViewer.DocumentParts;

public partial class FontPart: DocumentPart
{
    public FontPart(string title, PdfDictionary node, IReadOnlyList<DocumentPart> children) : base(title, children)
    {
    }
}