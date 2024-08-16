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

    public Stream WritingStream() => new MultiBufferStream(this, false, true, true, default);
    // we can use a default ticket here because it makes no sense for the writing stream to be the only
    // reference to the multibufferstream list.  Using a default ticket means that not disposing the writing
    // stream will not hold the buffers open.

    public CountingPipeWriter WritingPipe() => new CountingPipeWriter(PipeWriter.Create(WritingStream()));
}