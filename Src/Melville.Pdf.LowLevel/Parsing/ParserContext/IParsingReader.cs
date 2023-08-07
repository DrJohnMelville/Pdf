using System.IO.Pipelines;
using Melville.INPC;
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext;

internal partial class ParsingReader
{
    [FromConstructor] public ParsingFileOwner Owner { get; }
    [FromConstructor] public IByteSourceWithGlobalPosition Reader { get; }

    public ParsingReader(ParsingFileOwner owner, PipeReader reader, long lastSeek) :
        this(owner, new ByteSourceWithGlobalPosition(reader, lastSeek))
    {
    }
}