using System;
using Melville.INPC;
using Melville.Parsing.CountingReaders;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext;

internal partial class ParsingReader(ParsingFileOwner owner, ParsingBuffer buffer)
{
    public ParsingFileOwner Owner { get; } = owner;
    public ParsingBuffer Buffer { get; } = buffer;

    public virtual void Dispose() => Buffer.Dispose();
}

internal partial class DisposableParsingReader:ParsingReader, IDisposable
{
    private IDisposable? source;
    public DisposableParsingReader(ParsingFileOwner owner, ParsingBuffer buffer): base(owner, buffer)
    {
    }
    public DisposableParsingReader(ParsingFileOwner owner, IAsyncReader reader): 
        base(owner, new ParsingBuffer(reader))
    {
        source = reader as IDisposable;
    }

    public void Dispose()
    {
        Buffer.Dispose();
        source?.Dispose();
    }
}