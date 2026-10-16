using System;
using Melville.INPC;
using Melville.Parsing.CountingReaders;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext;

#warning eventually I want this to take an IMultiplexsource -- but right now I need to get tests to run
internal partial class ParsingReader(ParsingFileOwner owner, ParsingBuffer buffer, IByteSource source): IDisposable
{
    public ParsingReader(ParsingFileOwner owner, IByteSource source) :
        this(owner, new ParsingBuffer(new IByteSourceToIAsyncReadStream(source))
        {
            RelativePosition = source.Position
        }, source)
    { }

    public ParsingFileOwner Owner { get; } = owner;
    public IByteSource ObsoleteReader { get; } = source;
    public ParsingBuffer Buffer { get; } = buffer;

    public void Dispose()
    {
        ObsoleteReader.Dispose();
    }
}