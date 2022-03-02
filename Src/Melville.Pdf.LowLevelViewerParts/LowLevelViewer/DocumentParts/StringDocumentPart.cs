using Melville.Linq;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Streams;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts;

public class StringDocumentPart : DocumentPart
{
    private ByteStringViewModel stringDetail;
    public override object? DetailView => stringDetail;

    public StringDocumentPart(PdfString title, string prefix) : base($"{prefix}({title})")
    {
        stringDetail = new ByteStringViewModel(title.Bytes);
    }
}