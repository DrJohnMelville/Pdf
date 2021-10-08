using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.Wrappers
{
    public readonly struct PdfTree<T> where T : PdfObject, IComparable<T>
    {
        public PdfDictionary LowLevelSource { get; }

        public PdfTree(PdfDictionary lowLevelSource) : this()
        {
            LowLevelSource = lowLevelSource;
        }

        public ValueTask<PdfObject> Search(T key) => new TreeSearcher<T>(LowLevelSource, key).Search();
    }
}