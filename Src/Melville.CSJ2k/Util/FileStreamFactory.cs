// Copyright (c) 2007-2016 Melville.CSJ2K contributors.
// Licensed under the BSD 3-Clause License.

namespace Melville.CSJ2K.Util
{
    using System;
    using System.IO;

    public class FileStreamFactory
    {
        #region FIELDS

        private static IFileStreamCreator _creator;

        #endregion

        #region CONSTRUCTORS

        static FileStreamFactory()
        {
        }

        #endregion

        #region METHODS

        public static void Register(IFileStreamCreator creator)
        {
            _creator = creator;
        }

        internal static Stream New(string path, string mode)
        {
            if (_creator == null) throw new InvalidOperationException("No file stream creator is registered.");
            if (path == null) throw new ArgumentNullException("path");
            if (mode == null) throw new ArgumentNullException("mode");

            return _creator.Create(path, mode);
        }

        #endregion
    }
}
