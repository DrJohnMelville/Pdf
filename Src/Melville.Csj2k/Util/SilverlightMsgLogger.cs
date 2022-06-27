// Copyright (c) 2007-2016 CSJ2K contributors.
// Licensed under the BSD 3-Clause License.

namespace Melville.CSJ2K.Util
{
    using System.IO;
    using System.IO.IsolatedStorage;

    using Melville.CSJ2K.j2k.util;

    public class SilverlightMsgLogger : StreamMsgLogger
    {
        #region FIELDS

        private static readonly IMsgLogger Instance = new SilverlightMsgLogger();

        #endregion

        #region CONSTRUCTORS

        public SilverlightMsgLogger()
            : base(GetIsolatedFileStream("csj2k.out"), GetIsolatedFileStream("csj2k.out"), 78)
        {
        }

        #endregion

        #region METHODS

        public static void Register()
        {
            FacilityManager.DefaultMsgLogger = Instance;
        }

        private static Stream GetIsolatedFileStream(string fileName)
        {
            using (var isolatedFile = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return new IsolatedStorageFileStream(fileName, FileMode.Create, FileAccess.ReadWrite, isolatedFile);
            }
        }

        #endregion
    }
}
