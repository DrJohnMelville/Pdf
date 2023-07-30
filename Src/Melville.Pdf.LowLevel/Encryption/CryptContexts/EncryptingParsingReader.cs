using System;
using Melville.INPC;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Encryption.CryptContexts;


#warning -- get rid of this
[Obsolete("Can handle crypt contexts better with the pdf stack")]
internal partial class EncryptingParsingReader : IParsingReader
{
    [DelegateTo]
    private readonly IParsingReader inner;
    private readonly IObjectCryptContext cryptContext;

    public EncryptingParsingReader(IParsingReader inner, IObjectCryptContext cryptContext)
    {
        this.inner = inner;
        this.cryptContext = cryptContext;
    }

    public IObjectCryptContext ObjectCryptContext() => cryptContext;
}