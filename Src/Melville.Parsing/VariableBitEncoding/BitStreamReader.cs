using System.Buffers;
using System.IO.Pipelines;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Parsing.VariableBitEncoding;

/// <summary>
/// Reads a PipeReader as bits instead of bytes.
/// </summary>
public readonly struct BitStreamReader
{
    private readonly PipeReader pipe;
    private readonly BitReader reader;
    private readonly int bits;

    /// <summary>
    /// Construct a BitStreamReader.
    /// </summary>
    /// <param name="source">The data to be read.</param>
    /// <param name="bits">The number of bits to read in each number.</param>
    public BitStreamReader(Stream source, int bits) : this()
    {
        pipe = PipeReader.Create(source);
        reader = new BitReader();
        this.bits = bits;
    }

    /// <summary>
    /// Read a number from the stream.
    /// </summary>
    /// <returns>A number derived from the number of bits passed into the constructor.</returns>
    public async ValueTask<uint> NextNumAsync()
    {
        while (true)
        {
            var span = await pipe.ReadAsync().CA();
            if (TryRead(span.Buffer, out var ret)) return ret;
            pipe.AdvanceTo(span.Buffer.Start, span.Buffer.End);
        }
    }

    /// <summary>
    /// Try to read a number from the given sequence.
    /// </summary>
    /// <param name="spanBuffer">The source data.</param>
    /// <param name="output">the number composed the number of bits passed in the constructor
    /// taken from the source buffer</param>
    /// <returns>True if success, false if there is not enough data in the source sequence</returns>
    private bool TryRead(in ReadOnlySequence<byte> spanBuffer, out uint output)
    {
        var seqReader = new SequenceReader<byte>(spanBuffer);
        if (!reader.TryRead(bits, ref seqReader, out var intValue))
        {
            output = 0;
            return false;
        }
        pipe.AdvanceTo(seqReader.Position);
        output = (uint)intValue;
        return true;
    }
}