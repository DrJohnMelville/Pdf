using System.Numerics;
using Melville.INPC;
using Melville.Pdf.Model.Renderers.Bitmaps;

namespace Melville.Pdf.ImageExtractor
{
    internal unsafe partial class WrapNonExtractedBitmap : IExtractedBitmap
    {
        /// <summary>
        /// This is the inner bitmap being wrapped
        /// </summary>
        [FromConstructor] [DelegateTo] private readonly IPdfBitmap target;
        /// <summary>
        /// The display matrix at the time the image was painted.
        /// </summary>
        [FromConstructor] private readonly Matrix3x2 displayMatrix;
        [FromConstructor] public int Page { get; }

        public Vector2 PositionBottomLeft => TransformPoint(0, 0);
        public Vector2 PositionBottomRight => TransformPoint(1, 0);
        public Vector2 PositionTopLeft => TransformPoint(0, 1);
        public Vector2 PositionTopRight => TransformPoint(1, 1);
        private Vector2 TransformPoint(float x, float y) => Vector2.Transform(new Vector2(x, y), displayMatrix);

    }
}