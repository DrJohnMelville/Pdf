using CoreJ2K.Util;

namespace Melville.Pdf.LowLevel.Filters.JpxDecodeFilters
{
    internal class RawImage: ImageBase<RawImage>
    {
        public int Width { get; }
        public int Height { get; }
        public byte[] Bytes { get; }

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