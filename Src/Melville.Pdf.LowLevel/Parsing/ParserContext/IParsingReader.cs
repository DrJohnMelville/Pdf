using System;
using Melville.INPC;
using Melville.Parsing.CountingReaders;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext;

internal partial class ParsingReader: IDisposable
{
    [FromConstructor] public ParsingFileOwner Owner { get; }
    [FromConstructor] public IByteSource Reader { get; }

    public void Dispose()
    {
        Reader.Dispose();
    }
}