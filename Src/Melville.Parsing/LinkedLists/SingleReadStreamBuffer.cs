using Melville.INPC;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Streams;

namespace Melville.Parsing.LinkedLists;

[FromConstructor]
internal partial class SingleReadStreamBuffer: StreamBackedBuffer<SingleReadStreamBuffer>
{
    public static IMultiplexSource Create(Stream source, bool leaveOpen, int bufferSize = 4096)
    {
        var linkedList = new SingleReadStreamBuffer(source, leaveOpen).With(bufferSize);
        return new MultiBufferStream(linkedList, true, false, false);
    }

    public override void HasReadTo(SequencePosition consumed)
    {
        PruneFromFront(consumed);
    }

    public override void AddReference()
    {
        base.AddReference();
        if (references > 2)
            throw new InvalidOperationException(
                "SingleReadStreamBuffer should only have one reference");
    }
}