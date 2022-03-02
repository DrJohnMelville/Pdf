using System;
using System.Collections.Generic;
using Melville.INPC;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts;

public partial class DocumentPart
{
    public string Title { get; }
    public IReadOnlyList<DocumentPart> Children { get; }
    public virtual object? DetailView => null;

    public DocumentPart(string title):this(title, Array.Empty<DocumentPart>()) { }
    public DocumentPart(string title, IReadOnlyList<DocumentPart> children)
    {
        Title = title;
        Children = children;
    }
}