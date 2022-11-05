using System;
using Melville.INPC;
using Melville.Pdf.LowLevel.Encryption.CryptContexts;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;

namespace Melville.Pdf.LowLevel.Encryption.Cryptography.AesImplementation;

[StaticSingleton]
public partial class AesCipherFactory: ICipherFactory
{
    public ICipher CipherFromKey(in ReadOnlySpan<byte> finalKey) => new AesCipher(finalKey);
}