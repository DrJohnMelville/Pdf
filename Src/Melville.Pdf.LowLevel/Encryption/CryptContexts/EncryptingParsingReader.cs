using Melville.INPC;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Encryption.CryptContexts;

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