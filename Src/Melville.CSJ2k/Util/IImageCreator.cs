// Copyright (c) 2007-2016 Melville.CSJ2K contributors.
// Licensed under the BSD 3-Clause License.

namespace Melville.CSJ2K.Util
{
    using Melville.CSJ2K.j2k.image;

    internal interface IImageCreator : IDefaultable
    {

        BlkImgDataSrc ToPortableImageSource(object imageObject);
    }
}
