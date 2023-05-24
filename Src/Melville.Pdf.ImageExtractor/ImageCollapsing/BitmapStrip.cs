using Melville.INPC;
using Melville.Pdf.Model.Renderers.Bitmaps;
using System.Numerics;

namespace Melville.Pdf.ImageExtractor.ImageCollapsing
{
    public abstract partial class BitmapStrip : IExtractedBitmap
    {
        protected readonly List<IExtractedBitmap> componentBitmaps = new();

        public int Count => componentBitmaps.Count;

        public abstract int Width { get; }
        public abstract int Height { get; }
        public bool DeclaredWithInterpolation => FirstChild.DeclaredWithInterpolation;
        public int Page => FirstChild.Page;
        public abstract Vector2 PositionBottomLeft { get; }
        public abstract Vector2 PositionBottomRight { get; }
        public abstract Vector2 PositionTopLeft { get; }
        public abstract Vector2 PositionTopRight { get; }

        protected IExtractedBitmap FirstChild => componentBitmaps[0];
        protected IExtractedBitmap LastChild => componentBitmaps[componentBitmaps.Count -1];
    
        public BitmapStrip AddPrior(IExtractedBitmap prior)
        {
            componentBitmaps.Insert(0, prior);
            return this;
        }
        public BitmapStrip AddNext(IExtractedBitmap next)
        {
            componentBitmaps.Add(next);
            return this;
        }
        public BitmapStrip Concat(BitmapStrip next)
        {
            foreach (var item in next.componentBitmaps)
            {
                componentBitmaps.Add(item);
            }
            return this;
        }

        public static BitmapStrip Combine<T>(IExtractedBitmap prior, IExtractedBitmap next)
            where T: BitmapStrip, new() => (prior, next) switch
        {
            (BitmapStrip priorStrip, BitmapStrip nextStrip) => priorStrip.Concat(nextStrip),
            (BitmapStrip priorStrip, _) => priorStrip.AddNext(next),
            (_, BitmapStrip nextStrip) => nextStrip.AddPrior(prior),
            _=> new T().AddNext(prior).AddNext(next)
        };

        public unsafe ValueTask RenderPbgraAsync(byte* buffer) =>
            RenderPbgra(new PointerHolder(buffer));

        protected abstract ValueTask RenderPbgra(PointerHolder buffer);

        protected readonly unsafe partial struct PointerHolder
        {
            [FromConstructor] private readonly byte* root;

            public ValueTask WriteToStream(long offset, IPdfBitmap item) =>
                item.RenderPbgraAsync(root + offset);

            public byte* BasePointer() => root;
        }
    }
}