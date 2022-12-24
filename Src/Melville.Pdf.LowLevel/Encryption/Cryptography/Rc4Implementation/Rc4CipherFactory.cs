using System;
using Melville.INPC;
using Melville.Pdf.LowLevel.Encryption.CryptContexts;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;

namespace Melville.Pdf.LowLevel.Encryption.Cryptography.Rc4Implementation;

[StaticSingleton]
internal partial class Rc4CipherFactory : ICipherFactory
{
    public ICipher CipherFromKey(in ReadOnlySpan<byte> finalKey) =>
        new Rc4Cipher(finalKey);
}