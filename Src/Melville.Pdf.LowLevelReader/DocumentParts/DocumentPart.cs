using System;
using System.Collections.Generic;

namespace Melville.Pdf.LowLevelReader.DocumentParts
{
    public class DocumentPart
    {
        public string Title { get; }
        public IReadOnlyList<DocumentPart> Children { get; }

        public DocumentPart(string title):this(title, Array.Empty<DocumentPart>()) { }
        public DocumentPart(string title, IReadOnlyList<DocumentPart> children)
        {
            Title = title;
            Children = children;
        }
    }
}