// Copyright (c) 2007-2016 Melville.CSJ2K contributors.
// Licensed under the BSD 3-Clause License.

namespace Melville.CSJ2K.Util
{
    using Melville.CSJ2K.j2k.image;

    public interface IImageCreator : IDefaultable
    {
        IImage Create(int width, int height, byte[] bytes);

        BlkImgDataSrc ToPortableImageSource(object imageObject);
    }
}
