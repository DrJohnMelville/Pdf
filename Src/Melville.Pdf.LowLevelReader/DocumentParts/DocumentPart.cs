using System;
using System.Collections.Generic;
using Melville.INPC;

namespace Melville.Pdf.LowLevelReader.DocumentParts
{
    public partial class DocumentPart
    {
        public string Title { get; }
        [AutoNotify] private string[]? formats = null;
        public IReadOnlyList<DocumentPart> Children { get; }

        public DocumentPart(string title):this(title, Array.Empty<DocumentPart>()) { }
        public DocumentPart(string title, IReadOnlyList<DocumentPart> children)
        {
            Title = title;
            Children = children;
        }
    }
}