using CoreJ2K.Util;

namespace Melville.Pdf.LowLevel.Filters.JpxDecodeFilters
{
    internal class RawImage: ImageBase<RawImage>
    {
        public new int Width { get; }
        public new int Height { get; }
        public int Components { get; }
        public new byte[] Bytes { get; }

        /// <inheritdoc />
        public RawImage(int width, int height, int components, byte[] bytes) : 
            base(width, height, components, bytes)
        {
            Width = width;
            Height = height;
            Bytes = bytes;
            Components = components;
        }

        /// <inheritdoc />
        protected override object GetImageObject()
        {
            return this;
        }
    }
}