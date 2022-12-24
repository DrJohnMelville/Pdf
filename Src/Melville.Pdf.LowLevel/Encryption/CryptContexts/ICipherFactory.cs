using System;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;

namespace Melville.Pdf.LowLevel.Encryption.CryptContexts;

internal interface ICipherFactory
{
    ICipher CipherFromKey(in ReadOnlySpan<byte> finalKey);
}