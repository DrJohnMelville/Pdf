// Copyright (c) 2007-2016 CSJ2K contributors.
// Licensed under the BSD 3-Clause License.

namespace Melville.CSJ2K.Util
{
    using System;
    using System.IO;

    public class SilverlightFileStreamCreator : IFileStreamCreator
    {
        #region FIELDS

        private static readonly IFileStreamCreator Instance = new SilverlightFileStreamCreator();

        #endregion

        #region METHODS

        public static void Register()
        {
            FileStreamFactory.Register(Instance);
        }

        public Stream Create(string path, string mode)
        {
            throw new NotImplementedException("File stream I/O not implemented for Silverlight.");
        }

        #endregion
    }
}
