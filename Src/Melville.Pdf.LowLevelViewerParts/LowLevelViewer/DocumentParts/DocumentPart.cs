using Melville.INPC;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts;

public partial class DocumentPart
{
    public string Title { get; }
    [AutoNotify] private IReadOnlyList<DocumentPart> children;
    public virtual object? DetailView => null;

    public DocumentPart(string title):this(title, Array.Empty<DocumentPart>()) { }
    public DocumentPart(string title, IReadOnlyList<DocumentPart> children)
    {
        Title = title;
        this.children = children;
    }
}