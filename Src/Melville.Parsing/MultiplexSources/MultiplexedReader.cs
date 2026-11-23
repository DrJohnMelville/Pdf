using Melville.IntersectionTypes;
using Melville.StreamInterfaces;

namespace Melville.Parsing.MultiplexSources;

internal sealed class MultiplexedReader(MultiplexedReaderSource source) : CountedMultiplexSource
{
    public override long Length => throw new NotImplementedException();

    protected override Stream ReadFromOverride(long position)
    {
        throw new NotImplementedException();
    }
    protected override IMultiplexReader ReaderFromOverride(long position)
    {
        throw new NotImplementedException();
    }
    protected override void CleanUp()
    {
        throw new NotImplementedException();
    }
}

[IntersectionType]
public readonly partial struct MultiplexedReaderSource: 
    IAsyncReader, ISyncReader, ISeekableStream, IStreamLength { }
