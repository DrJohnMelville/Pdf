﻿// Copyright (c) 2007-2016 Melville.CSJ2K contributors.
// Licensed under the BSD 3-Clause License.

namespace Melville.CSJ2K.Util
{
    using System.IO;

    internal interface IFileStreamCreator
    {
        Stream Create(string path, string mode);
    }
}
