using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using Melville.StreamInterfaces.Streams;
using System.Buffers;
using System.IO.Pipelines;

namespace Melville.Parsing.VariableBitEncoding;

/// <summary>
/// Reads a PipeReader as bits instead of bytes.
/// </summary>
public readonly struct BitStreamReader: IDisposable
{
    private readonly ParsingBuffer pipe;
    private readonly BitReader reader;
    private readonly int bits;

    ///<inheritdoc/>
    public void Dispose() => pipe.Dispose();

    /// <summary>
    /// Construct a BitStreamReader.
    /// </summary>
    /// <param name="source">The data to be read.</param>
    /// <param name="bits">The number of bits to read in each number.</param>
    public BitStreamReader(Stream source, int bits) : this()
    {
        pipe = new ParsingBuffer(source.AsStreamReader());
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
            var spb = new SpanParsingBuffer(pipe.Peek());
            if (reader.TryRead(bits, ref spb, out var intValue))
            {
                pipe.Advance((int)spb.RelativePosition);
                return (uint)intValue;
            }
            await pipe.ReadBytesAsync();
        }
    }
}