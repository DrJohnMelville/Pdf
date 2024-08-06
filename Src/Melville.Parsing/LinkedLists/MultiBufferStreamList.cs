using System.Data.SqlTypes;
using System.IO.Pipelines;
using Melville.Parsing.Streams;
using Melville.Parsing.Streams.Bases;
using Melville.Parsing.Writers;

namespace Melville.Parsing.LinkedLists;

internal class MultiBufferStreamList : LinkedList<MultiBufferStreamList>, IWritableMultiplexSource
{
    public static MultiBufferStreamList WritableList(int blockSize)
    {
        var ret = new MultiBufferStreamList();
        ret.With(blockSize);
        return ret;
    }

    public static LinkedList SingleItemList(ReadOnlyMemory<byte> source) =>
        new MultiBufferStreamList().With(source);

    public Stream WritingStream() => new MultiBufferStream(this, false, true, true);

    public CountingPipeWriter WritingPipe() => new CountingPipeWriter(PipeWriter.Create(WritingStream()));
}