using CoreJ2K.Util;

namespace Melville.Pdf.LowLevel.Filters.JpxDecodeFilters
{
    internal class RawImage: ImageBase<RawImage>
    {
        public new int Width { get; }
        public new int Height { get; }
        public new byte[] Bytes { get; }

        /// <inheritdoc />
        public RawImage(int width, int height, byte[] bytes) : base(width, height, bytes)
        {
            Width = width;
            Height = height;
            Bytes = bytes;
        }

        /// <inheritdoc />
        protected override object GetImageObject()
        {
            return this;
        }
    }
}