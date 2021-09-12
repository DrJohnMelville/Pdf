using System;
using System.Security.Cryptography;

namespace Melville.Pdf.LowLevel.Encryption.Readers
{
    public static class HashAlgorithmHelper
    {
        public static void AddData(this HashAlgorithm ha, byte[] data)
        {
            ha.AddData(data, data.Length);
        }

        public static void AddData(this HashAlgorithm ha, byte[] data, int length)
        {
            ha.TransformBlock(data, 0, length, null, 0);
        }

        public static void FinalizeHash(this HashAlgorithm ha)
        {
            ha.TransformFinalBlock(Array.Empty<byte>(), 0,0);
        }
    }
}