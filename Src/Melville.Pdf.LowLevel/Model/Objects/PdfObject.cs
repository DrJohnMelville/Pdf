using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects
{
    public abstract class PdfObject
    {
        public virtual ValueTask<PdfObject> DirectValueAsync() => new(this);
        public abstract T Visit<T>(ILowLevelVisitor<T> visitor);
        public virtual bool ShouldWriteToFile() => true;
    }

    public static class PdfObjectOperations
    {
        public static IReadOnlyList<PdfObject> AsList(this PdfObject? item)
        {
            return item switch
            {
                PdfBoolean =>new []{item},
                null or PdfTokenValues => Array.Empty<PdfObject>(),
                PdfArray arr => arr.RawItems,
                _ => new[] {item}
            };
        }
    }
}