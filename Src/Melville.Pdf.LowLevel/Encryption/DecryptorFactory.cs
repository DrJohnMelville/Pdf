using System;
using Melville.Pdf.LowLevel.Parsing.Decryptors;

namespace Melville.Pdf.LowLevel.Encryption
{
    public interface IDecryptorFactory
    {
        IDecryptor ComputeDecryptor(byte baseKey, int objectNumber, int generationNumber)
        {
            throw new NotImplementedException();
        }
    }
}