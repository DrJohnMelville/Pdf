using System.IO.Pipelines;
using Melville.Hacks;

namespace Melville.Parsing.VariableBitEncoding;

/// <summary>
/// Pack bits into bytes and write to a stream and 
/// </summary>
public readonly struct BitStreamWriter
{
    private readonly PipeWriter pipe;
    private readonly BitWriter writer;
    private readonly int bits;

    /// <summary>
    /// Create a BitStreamWriter
    /// </summary>
    /// <param name="destination">The pipewriter to write to.</param>
    /// <param name="bits">Number of bits per number to write.</param>
    public BitStreamWriter(Stream destination, int bits)
    {
        this.bits = bits;
        pipe = PipeWriter.Create(destination);
        writer = new BitWriter();
    }

    /// <summary>
    /// Write the number to the stream witih the given number of bits.
    /// </summary>
    /// <param name="datum">The number to write.</param>
    public void Write(uint datum)
    {
        var span = pipe.GetSpan(5);
        pipe.Advance(writer.WriteBits(datum, bits, span));
    }
        
    /// <summary>
    /// Flush the remaining bits to the output stream
    /// </summary>
    public ValueTask FinishAsync()
    {
        var span = pipe.GetSpan(1);
        pipe.Advance(writer.FinishWrite(span));
        return pipe.FlushAsync().AsValueTask();
    }
}