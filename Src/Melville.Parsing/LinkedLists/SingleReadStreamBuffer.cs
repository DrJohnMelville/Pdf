using Melville.INPC;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Streams;

namespace Melville.Parsing.LinkedLists;

[FromConstructor]
internal partial class SingleReadStreamBuffer: StreamBackedBuffer<SingleReadStreamBuffer>
{
    public static IMultiplexSource Create(Stream source, bool leaveOpen, int bufferSize = 4096) => 
        new SingleReadStreamBuffer(source, leaveOpen).With(bufferSize);

    public override void HasReadTo(SequencePosition consumed)
    {
        PruneFromFront(consumed);
    }
}