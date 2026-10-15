using System;
using Melville.INPC;
using Melville.Parsing.CountingReaders;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext;

#warning eventually I want this to take an IMultiplexsource -- but right now I need to get tests to run
internal partial class ParsingReader(ParsingFileOwner owner, IByteSource reader): IDisposable
{
    public ParsingFileOwner Owner { get; } = owner;
    public IByteSource Reader { get; } = reader;
    public ParsingBuffer Buffer { get; } = new ParsingBuffer(new IByteSourceToIAsyncReadStream(reader))
    {
        // this is a hack -- reader need to go away and take everything from the buffer.
        RelativePosition = reader.Position
    };


    public void Dispose()
    {
        Reader.Dispose();
    }
}