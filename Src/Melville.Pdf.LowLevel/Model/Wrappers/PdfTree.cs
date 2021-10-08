using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

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

    public readonly struct TreeSearcher<T> where T:PdfObject, IComparable<T>
    {
        private readonly PdfDictionary source;
        private readonly T key;
        
        public TreeSearcher(PdfDictionary source, T key)
        {
            this.source = source;
            this.key = key;
        }

        public ValueTask<PdfObject> Search() =>
            source.ContainsKey(KnownNames.Kids) ? SearchInteriorNode() : SearchLeaf();

        private async ValueTask<PdfObject> SearchInteriorNode()
        {
            var kids = await source.GetAsync<PdfArray>(KnownNames.Kids);
            foreach (var kidTask in kids)
            {
                var kid = (PdfDictionary)await kidTask;
                var limits = await kid.GetAsync<PdfArray>(KnownNames.Limits);
                var low = await limits.GetAsync<T>(0);
                var high = await limits.GetAsync<T>(1);
                if (low.CompareTo(key) <= 0 && high.CompareTo(key) >= 0)
                {
                    return await new TreeSearcher<T>(kid, key).Search();
                }
            }
            throw NotFoundException();
        }

        private async ValueTask<PdfObject> SearchLeaf()
        {
            var array = await source.GetAsync<PdfArray>(PdfTreeElementNamer.FinalArrayName<T>());
            for (int i = 0; i < array.Count; i+= 2)
            {
                if (KeyMatchesTarget((T)await array[i])) return await array[i + 1];
            }
            throw NotFoundException();
        }

        private bool KeyMatchesTarget(T? arrayKey) => key.CompareTo(arrayKey) == 0;

        private Exception NotFoundException()
        {
            return new PdfParseException($"Cannot find item {key} in the PdfTree.");
        }
    }
}