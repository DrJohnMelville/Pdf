using System;
using CoreJ2K.j2k.image;
using CoreJ2K.Util;
using Melville.INPC;

namespace Melville.Pdf.LowLevel.Filters.JpxDecodeFilters
{
    [StaticSingleton]
    internal partial class RawImageCreator : IImageCreator
    {
        /// <inheritdoc />
        public bool IsDefault => false;

        public Type ImageType => typeof(RawImage);

        /// <inheritdoc />
        public IImage Create(int width, int height, int numComponents, byte[] bytes)
        {
            return new RawImage(width, height, numComponents, bytes);
        }

        /// <inheritdoc />
        public BlkImgDataSrc ToPortableImageSource(object imageObject)
        {
            throw new NotImplementedException();
        }
    }
}